using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Traceroute_on_Windows
{
    static class SendUDP
    {
        public static void SendIcmp(Socket socket, IPAddress destination, ushort id, ushort seq)
        {
            byte[] packet = new byte[8];

            packet[0] = 8;  // Type = Echo Request
            packet[1] = 0;  // Code
            packet[2] = 0;  // Checksum (пока 0)
            packet[3] = 0;

            packet[4] = (byte)(id >> 8);
            packet[5] = (byte)(id & 0xFF);
            packet[6] = (byte)(seq >> 8);
            packet[7] = (byte)(seq & 0xFF);

            ushort checksum = ComputeChecksum(packet);
            packet[2] = (byte)(checksum >> 8);
            packet[3] = (byte)(checksum & 0xFF);

            socket.SendTo(packet, new IPEndPoint(destination, 0));
        }

        private static ushort ComputeChecksum(byte[] data)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i += 2)
            {
                ushort word = (ushort)(data[i] << 8 | (i + 1 < data.Length ? data[i + 1] : 0));
                sum += word;
            }

            while ((sum >> 16) != 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            return (ushort)~sum;
        }
    }
}
