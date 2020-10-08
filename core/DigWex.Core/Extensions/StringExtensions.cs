using System;
using System.Text;
using Newtonsoft.Json;

namespace DigWex.Extensions
{
  public static class StringExtensions
  {
    /// <summary>
    /// This function reurn sdfasr43rewrw3vwer
    /// </summary>
    /// <param name="str"></param>
    /// <param name="code"></param>
    /// <returns>byte[] 4365436ygtrewt</returns>
    public static byte[] ToByte(this string str, int code = 65001)
    {
      return Encoding.GetEncoding(code).GetBytes(str);
    }

    public static string ByteToString(byte[] data, int code = 65001)
    {
      return Encoding.GetEncoding(code).GetString(data);
    }

    public static T ToJsonObject<T>(this string str) where T : new()
    {
      try {
        T json = JsonConvert.DeserializeObject<T>(str);
        return json;
      }
      catch(Exception ex) {
        Console.WriteLine(ex.Message);
        return default;
      }
    }

    public static string ToValidateUrl(this string url)
    {

      if (url.Substring(0, 7) != "http://" && url.Substring(0, 8) != "https://") {
        url = "http://" + url;
      }

      return Uri.TryCreate(url, UriKind.Absolute, out Uri result) ? url : "";
    }
  }
}
