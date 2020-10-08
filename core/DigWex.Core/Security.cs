using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using DigWex.Helpers;
using DigWex.Extensions;
using System.Threading.Tasks;
using VvHelpers.Extensions;

namespace DigWex
{
  public class Security
  {
    public T OpenProtectedData<T>(string localUrl) where T : new()
    {
      try {
        string json;
        string hash;
        StringBuilder builder = new StringBuilder();
        using (StreamReader sr = new StreamReader(localUrl, Encoding.UTF8)) {
          hash = sr.ReadLine();
          string line;
          while ((line = sr.ReadLine()) != null)
            builder.Append(line);
        }
        json = builder.ToString();

        bool ok = CryptoHelper.VerifyMd5Hash(json + Config.SignData, hash);

        //Console.WriteLine("Verify md5: {0}", ok);

        if (ok) {
          return JsonConvert.DeserializeObject<T>(json);
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
      return default;
    }

    public async Task<T> OpenProtectedDataAsync<T>(string localUrl) where T : new()
    {
      try {
        string json;
        //string hash;
        var builder = new StringBuilder();
        using (var sr = new StreamReader(localUrl, Encoding.UTF8)) {
          //hash = await sr.ReadLineAsync();
          string line;
          while ((line = await sr.ReadLineAsync()) != null)
            builder.Append(line);
        }
        json = builder.ToString();

        //bool ok = CryptoHelper.VerifyMd5Hash(json + Config.SignData, hash);

        //Console.WriteLine("Verify md5: {0}", ok);

        //if (ok) {
        return JsonConvert.DeserializeObject<T>(json);
        //}
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
      return default;
    }

    public bool SaveProtectedData(object obj, string localUrl)
    {
      byte[] sufix = Config.SignData.ToByte();
      byte[] prefix = obj.ToJsonByte();

      try {
        byte[] data = new byte[prefix.Length + sufix.Length];

        prefix.CopyTo(data, 0);
        sufix.CopyTo(data, prefix.Length);

        string json = Extensions.StringExtensions.ByteToString(prefix);
        string hash = CryptoHelper.Md5(data);
        //data = hash.ToByte();
        FileStream fs = new FileStream(localUrl, FileMode.Create, FileAccess.Write);
        using (StreamWriter stream = new StreamWriter(fs, Encoding.UTF8)) {
          stream.WriteLine(hash);
          stream.Write(json);
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
        return false;
      }
      return true;
    }
  }
}
