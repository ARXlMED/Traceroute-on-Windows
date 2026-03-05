using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traceroute_on_Windows
{
    internal class Core : IDisposable
    {
        private readonly IPAddress _destination;
        private Socket _socket;
        private const int MaxHops = 30;
        private const int Timeout = 2000;

        public Core(IPAddress destination)
        {
            _destination = destination;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            _socket.ReceiveTimeout = Timeout;
            _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        public void Run(Action<IPAddress?, long?> onHopResult)
        {
            ushort id = (ushort)Environment.ProcessId;
            ushort seq = 0;

            byte[] buffer = new byte[8192];

            for (int ttl = 1; ttl <= MaxHops; ttl++)
            {
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl);
                bool reached = false;

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    seq++;

                    Stopwatch sw = Stopwatch.StartNew();
                    SendIcmp.SendPacket(_socket, _destination, id, seq);

                    try
                    {
                        int received = _socket.Receive(buffer);
                        sw.Stop();

                        if (ParseICMP.TryParse(buffer, received, id, out IPAddress responder, out bool final))
                        {
                            onHopResult(responder, sw.ElapsedMilliseconds);
                            if (final)
                                reached = true;
                        }
                        else
                        {
                            onHopResult(null, null);
                        }
                    }
                    catch (SocketException)
                    {
                        onHopResult(null, null);
                    }
                }

                if (reached)
                    break;
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    
    }
}
