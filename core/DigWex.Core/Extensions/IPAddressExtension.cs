using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DigWex.Extensions
{
    public static class IPAddressExtension
    {
        public static byte IndexRemotePc(this IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            return bytes[3];
        }

        public static IPAddress BrodcastAddress()
        {
            IPAddress[] ips = Dns.GetHostEntry("").AddressList;
            byte[] brodcastIp = new byte[4];
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    brodcastIp = ip.GetAddressBytes();
            }
            brodcastIp[3] = 255;
            return new IPAddress(brodcastIp);
        }

        public static IPAddress LocalAddress()
        {
            IPAddress[] ips = Dns.GetHostEntry("").AddressList;
            List<IPAddress> res = new List<IPAddress>();
            foreach (IPAddress ip in ips)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            return null;
        }
    }
}
