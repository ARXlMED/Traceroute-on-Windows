using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Traceroute_on_Windows
{
    static class ParseICMP
    {
        public static bool IsResponseForProbe(byte[] packet)
        {
            return false;
        }
    }
}
