using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traceroute_on_Windows
{
    static class ParseICMP
    {
        public static bool IsResponseForProbe(byte[] packet, int lengthPacket, int expectedDestPort, IPAddress expectedDestAddr)
        {
            if (lengthPacket < 28) return false;
            
            int ipHeaderLen = (packet[0] & 0x0F) * 4;
            if (lengthPacket < ipHeaderLen + 8) return false;

            int icmpOffset = ipHeaderLen;
            byte icmpType = packet[icmpOffset];
            if (icmpType != 11 && icmpType != 3) return false;

            int innerIpOffset = icmpOffset + 8;
            if (innerIpOffset + 20 > lengthPacket) return false;

            byte[] innerDestAddrBytes = new byte[4];
            Array.Copy(packet, innerIpOffset + 16, innerDestAddrBytes, 0, 4);
            IPAddress innerDestAddr = new IPAddress(innerDestAddrBytes);
            if (!innerDestAddr.Equals(expectedDestAddr)) return false;

            byte innerProtocol = packet[innerIpOffset + 9];
            if (innerProtocol != 17) return false;

            int innerUdpOffset = innerIpOffset + ((packet[innerIpOffset] & 0x0F) * 4);
            if (innerUdpOffset + 2 > lengthPacket) return false;

            int innerDestPort = (packet[innerUdpOffset] << 8) | packet[innerUdpOffset + 1];
            return innerDestPort == expectedDestPort;
        }
    }
}
