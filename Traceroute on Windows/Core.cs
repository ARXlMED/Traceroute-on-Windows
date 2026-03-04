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
        private const int standartPortUDP = 33434;
        private const int maxTTL = 30;
        private const int probesForOneTTL = 3;

        private Socket icmpSocket;
        private Socket udpSocket;
        private EndPoint remoteNowEndPoint;
        private IPEndPoint finalPoint;

        private byte[] buffer;
        public Core(IPAddress finalIP) 
        {
            ChangeDestinition(finalIP);
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            icmpSocket.ReceiveTimeout = 3000;
            icmpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            icmpSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, null);
            remoteNowEndPoint = new IPEndPoint(IPAddress.Any, 0);

            buffer = new byte[8192];
        }

        public void Dispose()
        {
            udpSocket?.Dispose();
            icmpSocket?.Dispose();
        }

        public void ChangeDestinition(IPAddress finalIP)
        {
            if (finalIP == null) throw new ArgumentNullException(nameof(finalIP));
            finalPoint = new IPEndPoint(finalIP, standartPortUDP);
        }

        public void Run(Action<IPAddress?, int?> hopHandler)
        {
            bool reachedDestination = false;
            for (int ttl = 1; ttl <= maxTTL; ttl++)
            {
                if (reachedDestination) break;
                Console.Write($"{ttl,2}  ");
                for (int probe = 0; probe < probesForOneTTL; probe++)
                {
                    int port = standartPortUDP + (ttl * probesForOneTTL) + probe;
                    var sw = Stopwatch.StartNew();
                    SendUDP.SendProbe(udpSocket, finalPoint.Address, port, ttl);
                    IPAddress? responder = TryRecieveResponse(port);
                    sw.Stop();
                    int? rtt = responder == null ? null : (int)sw.ElapsedMilliseconds;
                    hopHandler(responder, rtt);
                    if (responder != null && responder.Equals(finalPoint.Address))
                    {
                        reachedDestination = true;
                        break;
                    }
                    Thread.Sleep(100);
                }
                Console.WriteLine();
            }
        }

        public IPAddress? TryRecieveResponse(int expectedPort)
        {
            while (true)
            {
                try
                {
                    int lengthPacket = icmpSocket.ReceiveFrom(buffer, ref remoteNowEndPoint);
                    IPAddress responder = ((IPEndPoint)remoteNowEndPoint).Address;
                    if (ParseICMP.IsResponseForProbe(buffer, lengthPacket, expectedPort, finalPoint.Address))
                    {
                        return responder;
                    }

                }
                catch (SocketException se) when (se.SocketErrorCode == SocketError.TimedOut) 
                {
                    return null;
                }
            }
        }
    }
}
