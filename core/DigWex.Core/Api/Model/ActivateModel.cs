using Newtonsoft.Json;

namespace DigWex.Api.Model
{
  public class ActivateModel 
  {
    public class Request
    {
      [JsonRequired]
      [JsonProperty("pin")]
      public string Pin { get; set; }

      [JsonProperty("platform")]
      public string Platform { get; set; }
    }

    public class Response
    {
      [JsonRequired]
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonRequired]
      [JsonProperty("access_token")]
      public string Token { get; set; }

      [JsonProperty("offset")]
      public int Offset { get; set; }

      [JsonProperty("settings")]
      public SettingsModel Settings { get; set; }
    }

  }
}
