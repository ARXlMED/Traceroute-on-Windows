using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Traceroute_on_Windows
{
    public static class DnsResolver
    {
        public static string Resolve(IPAddress address)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(address);
                return entry.HostName;
            }
            catch
            {
                return address.ToString();
            }
        }
    }
}
