using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DigWex.Utils
{
  public static class NetworkUtils
  {
    public static PhysicalAddress GetMacAddress()
    {
      try {
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
          if ((nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
              nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
              nic.OperationalStatus == OperationalStatus.Up)
            return nic.GetPhysicalAddress();
        }
      }
      catch { }
      return null;
    }



    public static long LocalAddress()
    {
      try {
        var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        foreach (var ip in ips) {
          if (ip.AddressFamily == AddressFamily.InterNetwork) {
            byte[] bytes = ip.GetAddressBytes();
            long res = bytes[0];
            res <<= 24;
            res |= bytes[1];
            res <<= 16;
            res |= bytes[2];
            res <<= 8;
            res |= bytes[3];
            return res;
          }
        }
      }
      catch { }
      return -1;
    }
  }
}
