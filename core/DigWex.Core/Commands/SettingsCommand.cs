using DigWex.Api.Model;
using DigWex.Extensions;
using DigWex.Network;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DigWex.Commands
{
  public class SettingsCommand : Command
  {
    private JToken _extra;

    public SettingsCommand(JToken extra)
    {
      _extra = extra;
    }

    public async override void StartAsync()
    {
      Log.Info($"Start settings command, id: {CommandId}");

      bool ok = await ChangeSettings();

      if (ok)
        Log.Info($"Complete settings command, id: {CommandId}");
      else
        Log.Info($"Error settings command, id: {CommandId}");
    }

    private async Task<bool> ChangeSettings()
    {
      try {
        SettingsModel model = _extra.ToObject<SettingsModel>();
        string str = "settings";
        if (model != null)
          await HttpServer.Instance.SendCommandAsync(new { str, model });
        return true;
      }
      catch { return false; }
    }
  }
}
