using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigWex.Api.Model;

namespace DigWex.Managers
{
  public sealed class SentryManager
  {
    private const string DSN = "https://2e30fc7e345649a791ab1100fc570cca:2ca99dd07c87436b850dd2737ac9e7fc@sentry.io/106910";

    private static readonly Lazy<SentryManager> _instance =
       new Lazy<SentryManager>(() => new SentryManager());

    private readonly RavenClient _ravenClient;

    public static SentryManager Instance {
      get {
        return _instance.Value;
      }
    }

    public void Init() { }

    private SentryManager()
    {

//#if ADVELIT
//      string a = App.Version;
//#endif
      _ravenClient = new RavenClient(DSN) {

#if RELEASE
                Release = "1.0.0",
                Logger = "client",
#else
        Logger = "develop",
#endif
        Timeout = TimeSpan.FromSeconds(5),

        ErrorOnCapture = error => { },
        BeforeSend = requester => {
          //requester.Packet.F
          requester.Packet.Extra = null;
          if (Config.ActivateData != null) {
            ActivateModel.Response config = Config.ActivateData;
            requester.Packet.Extra = new {
              //backend_url = config.BackendUrl,
              device_id = config.Id,
              offset = config.Offset
              //timezone = config.Timezone,
              //mode = config.Wall != null
              //      ? "wall" : config.Showcase != null
              //      ? "showcase" : "default"
            };
          }

          requester.Packet.Fingerprint = new string[] { "{{ default }}", Program.Version };
          requester.Packet.Culprit = null;
          requester.Packet.Modules = null;
          requester.Packet.ServerName = null;
          requester.Packet.User = null;
          //requester.Packet.Message = "Title";
          // Here you can log data from the requester
          // or replace it entirely if you want.
          return requester;
        }
      };
    }

    public async Task FatalAsync(Exception exception)
    {
      var ev = new SentryEvent(exception) {
        Level = ErrorLevel.Fatal,
        Tags = new Dictionary<string, string> {
          ["sign"] = Config.SignMd5
        }
      };
      await _ravenClient.CaptureAsync(ev);
    }

    public void Fatal(Exception exception)
    {
      var ev = new SentryEvent(exception);

      Console.WriteLine(exception.TargetSite);

      //ev.Fingerprint.Add(App.Version);

      ev.Level = ErrorLevel.Fatal;
      ev.Tags = new Dictionary<string, string> {
        ["sign"] = Config.SignMd5
      };
      _ravenClient.Capture(ev);
    }
  }
}
