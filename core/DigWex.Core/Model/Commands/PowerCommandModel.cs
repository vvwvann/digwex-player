using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigWex.Model.Commands
{
  public class PowerCommandModel : CommandModel<PowerKwargs>
  {
    public const string Type = "set-power";
  }

  public class SynchronizeCommandModel : CommandModel<EmptyKwargs>
  {
    public const string Type = "synchronize";

    public SynchronizeCommandModel(int id)
    {
      Command = Type;
      Id = id;
    }
  }

  public class TakeScreenshotCommandModel : CommandModel<EmptyKwargs>
  {
    public const string Type = "take-screenshot";
  }

  public class UpdataDataCommandModel : CommandModel<UpdataDataKwargs>
  {
    public const string Type = "update-data";
  }

  public class UploadLogsCommandModel : CommandModel<EmptyKwargs>
  {
    public const string Type = "upload-logs";
  }

  public class DistUrlCommandModel : CommandModel<UrlKwargs>
  {
    public const string Type = "set-dist-url";
  }

  public class BackendUrlCommandModel : CommandModel<UrlKwargs>
  {
    public const string Type = "set-backend-url";
  }

  public class RebootCommandModel : CommandModel<EmptyKwargs>
  {
    public const string Type = "reboot";
  }

  public class PowerKwargs : IKWargs
  {
    [JsonRequired]
    [JsonProperty("on")]
    public bool On { get; set; }
  }

  public class UrlKwargs : IKWargs
  {
    [JsonRequired]
    [JsonProperty("url")]
    public string Url { get; set; }
  }

  public class UpdataDataKwargs : IKWargs
  {
    [JsonRequired]
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("data")]
    public JToken Data {
      get {
        return _data;
      }
      set {
        _data = value;
      }
    }

    public JToken _data;
  }

  public class EmptyKwargs : IKWargs
  {

  }
}
