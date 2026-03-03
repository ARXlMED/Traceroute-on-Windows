using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Traceroute_on_Windows
{
    static class SendUDP
    {
        public static void SendProbe(Socket socket, IPAddress destinition, int port, int ttl)
        {
            socket.Ttl = (short)ttl;
            byte[] data = Encoding.ASCII.GetBytes("Hiii, it's traceroute");
            IPEndPoint endPoint = new IPEndPoint(destinition, port);
            socket.SendTo(data, endPoint);
        }
    }
}
