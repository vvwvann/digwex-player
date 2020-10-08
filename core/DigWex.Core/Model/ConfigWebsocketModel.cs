using DigWex.Api.Model;
using Newtonsoft.Json;

namespace DigWex.Model
{
  public class ConfigWebsocketModel
  {
    [JsonProperty("rebootId")]
    public long LastRebootId { get; set; }

    [JsonRequired]
    [JsonProperty("config")]
    public ConfigElectron Config { get; set; }

    public ScreenBound screen { get; set; }

    [JsonRequired]
    [JsonProperty("activate")]
    public ActivateModel.Response Activate { get; set; }
  }

  public class ConfigElectron
  {
    [JsonRequired]
    [JsonProperty("serverUrl")]
    public string ServerUrl { get; set; }

    [JsonProperty("contentPath")]
    public string ContentPath { get; set; }

    [JsonRequired]
    [JsonProperty("appData")]
    public string AppData { get; set; }
  }

  public class ScreenBound {
    public int x { get; set; }

    public int y { get; set; }

    public int width { get; set; }

    public int height { get; set; }
  }

}
