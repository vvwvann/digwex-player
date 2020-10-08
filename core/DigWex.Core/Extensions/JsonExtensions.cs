using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace DigWex.Extensions
{
  public static class JsonExtensions
  {
    public static string ToJsonString(this object obj, Formatting format = Formatting.None)
    {
      string str = JsonConvert.SerializeObject(obj,
          new JsonSerializerSettings {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = format
          });
      return str;
    }

    public static byte[] ToByte(this JObject obj)
    {
      string str = obj.ToString(Formatting.None);
      byte[] data = str.ToByte();
      return data;
    }

    public static byte[] ToJsonByte(this object obj, Formatting format = Formatting.None)
    {
      string str = obj.ToJsonString(format);
      byte[] data = str.ToByte();
      return data;
    }

    public static bool JsonToFile(this object obj, string localUrl)
    {
      byte[] data = obj.ToJsonByte();
      try {
        using (FileStream fileStream = new FileStream(localUrl, FileMode.Create, FileAccess.Write))
          fileStream.Write(data, 0, data.Length);
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
        return false;
      }
      return true;
    }

    public static async Task<bool> JsonToFileAsync(this object obj, string localUrl, Formatting format = Formatting.None)
    {
      byte[] data = obj.ToJsonByte(format);
      FileStream fileStream = null;
      try {
        fileStream = new FileStream(localUrl, FileMode.Create, FileAccess.Write);
        await fileStream.WriteAsync(data, 0, data.Length);
        fileStream.Close();
        return true;
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
      finally {
        fileStream?.Dispose();
      }
      return false;
    }

    public static async Task<T> LoadJsonFromFileAsync<T>(string localUrl) where T : new()
    {
      try {
        using (StreamReader r = new StreamReader(localUrl)) {
          string json = await r.ReadToEndAsync();
          T obj = JsonConvert.DeserializeObject<T>(json);
          return obj;
        }
      }
      catch (Exception ex) {
#if DEBUG
        Console.WriteLine(ex.Message);
#endif
        return default;
      }
    }

    public static JToken LoadJsonFromFile(string path)
    {
      try {
        using (StreamReader r = new StreamReader(path)) {
          string json = r.ReadToEnd();
          return JToken.Parse(json);
        }
      }
      catch (Exception ex) {
#if DEBUG
        Console.WriteLine(ex.Message);
#endif
        return null;
      }
    }
  }
}
