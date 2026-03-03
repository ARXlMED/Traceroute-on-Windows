using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traceroute_on_Windows
{
    internal class Core : IDisposable
    {
        private const int standartPortUDP = 33434; // потом можно сделать если не возвращает что порт недоступен на следующий тогда переключалось

        private Socket icmpSocket;
        private Socket udpSocket;
        private EndPoint remoteNowEndPoint;
        private EndPoint finalPoint;
        public Core(IPAddress finalIP) 
        {
            ChangeDestinition(finalIP);
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            icmpSocket.ReceiveTimeout = 3000;
            icmpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            icmpSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, null);
            remoteNowEndPoint = new IPEndPoint(IPAddress.Any, 0);

        }

        public void Dispose()
        {
            udpSocket?.Dispose();
            icmpSocket?.Dispose();
        }

        public void ChangeDestinition(IPAddress finalIP)
        {
            if (finalIP == null) { throw new ArgumentNullException(nameof(finalIP)); }
            finalPoint = new IPEndPoint(finalIP, standartPortUDP);
        }
    }
}
