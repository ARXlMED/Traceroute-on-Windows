using System;
using System.Net;

namespace Traceroute_on_Windows
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            bool resolveNames = true;
            string target;

            if (args[0] == "-d")
            {
                resolveNames = false;
                if (args.Length < 2)
                {
                    PrintUsage();
                    return;
                }
                target = args[1];
            }
            else
            {
                target = args[0];
            }

            try
            {
                IPAddress destination;

                if (!IPAddress.TryParse(target, out destination))
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(target);
                    destination = GetIPv4(hostEntry);
                }

                Console.WriteLine($"Tracing route to {target} [{destination}]");
                Console.WriteLine("over a maximum of 30 hops:\n");

                using (var core = new Core(destination))
                {
                    int ttl = 1;
                    int attempt = 0;

                    long?[] rtts = new long?[3];
                    IPAddress responder = null;

                    core.Run((addr, rtt) =>
                    {
                        rtts[attempt] = rtt;

                        if (addr != null)
                            responder = addr;

                        attempt++;

                        if (attempt == 3)
                        {
                            PrintHop(ttl, rtts, responder, resolveNames);

                            ttl++;
                            attempt = 0;
                            rtts = new long?[3];
                            responder = null;
                        }
                    });
                }

                Console.WriteLine("\nTrace complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void PrintHop(int ttl, long?[] rtts, IPAddress responder, bool resolveNames)
        {
            Console.Write($"{ttl,2}  ");

            for (int i = 0; i < 3; i++)
            {
                if (rtts[i].HasValue)
                {
                    if (rtts[i].Value < 1)
                        Console.Write("   <1 ms ");
                    else
                        Console.Write($"{rtts[i].Value,6} ms ");
                }
                else
                {
                    Console.Write("     *   ");
                }
            }

            if (responder != null)
            {
                if (resolveNames)
                {
                    string name = DnsResolver.Resolve(responder);
                    if (name != responder.ToString())
                        Console.Write($"  {name} [{responder}]");
                    else
                        Console.Write($"  {responder}");
                }
                else
                {
                    Console.Write($"  {responder}");
                }
            }
            else
            {
                Console.Write("  Request timed out.");
            }

            Console.WriteLine();
        }

        static IPAddress GetIPv4(IPHostEntry entry)
        {
            foreach (var addr in entry.AddressList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return addr;
            }

            throw new Exception("No IPv4 address found.");
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: traceroute.exe [-d] <hostname or IP>");
            Console.WriteLine("  -d    Disable reverse DNS lookup (show only IP addresses)");
        }
    }
}