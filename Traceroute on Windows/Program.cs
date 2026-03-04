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
                IPHostEntry hostEntry = Dns.GetHostEntry(target);
                IPAddress destination = hostEntry.AddressList[0];
                Console.WriteLine($"Tracing route to {target} [{destination}] over a maximum of 30 hops:\n");

                using (var core = new Core(destination))
                {
                    core.Run((responder, rtt) =>
                    {
                        if (responder == null)
                        {
                            Console.Write("   *   ");
                        }
                        else
                        {
                            if (rtt.HasValue)
                                Console.Write($"{rtt.Value,3} ms  ");
                            else
                                Console.Write("   ?   ");

                
                            string addrString;
                            if (resolveNames)
                            {
                                string name = DnsResolver.Resolve(responder);
                                if (name != responder.ToString())
                                    addrString = $"{name} [{responder}]";
                                else
                                    addrString = responder.ToString();
                            }
                            else
                            {
                                addrString = responder.ToString();
                            }
                            Console.Write($"{addrString,-30}  ");
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

        static void PrintUsage()
        {
            Console.WriteLine("Usage: traceroute.exe [-d] <hostname or IP>");
            Console.WriteLine("  -d    Disable reverse DNS lookup (show only IP addresses)");
        }
    }
}
