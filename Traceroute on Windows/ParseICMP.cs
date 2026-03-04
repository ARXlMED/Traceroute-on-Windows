using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traceroute_on_Windows
{
    static class ParseICMP
    {
        public static bool TryParse(byte[] buffer, int length, ushort id, out IPAddress responder, out bool reached)
        {
            responder = null;
            reached = false;

            if (length < 20)
                return false;

            int ipHeaderLen = (buffer[0] & 0x0F) * 4;
            if (length < ipHeaderLen + 8)
                return false;

            int icmpOffset = ipHeaderLen;
            byte type = buffer[icmpOffset];

            if (type == 11)
            {
                responder = new IPAddress(new byte[]
                {
                    buffer[12], buffer[13], buffer[14], buffer[15]
                });
                return true;
            }

            if (type == 0)
            {
                ushort replyId = (ushort)((buffer[icmpOffset + 4] << 8) | buffer[icmpOffset + 5]);
                if (replyId == id)
                {
                    responder = new IPAddress(new byte[]
                    {
                        buffer[12], buffer[13], buffer[14], buffer[15]
                    });
                    reached = true;
                    return true;
                }
            }

            return false;
        }
    }
}
