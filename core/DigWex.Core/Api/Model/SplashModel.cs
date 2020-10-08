using Newtonsoft.Json;

namespace DigWex.Api.Model
{
  public class SplashModel
  {
    [JsonProperty("img")]
    public Img Image { get; set; }

    [JsonProperty("background")]
    public string Background { get; set; }

    public class Img
    {
      [JsonProperty("url")]
      public string Url { get; set; }

      [JsonProperty("size")]
      public long Size { get; set; }

      [JsonProperty("md5")]
      public string Md5 { get; set; }
    }
  }
}
