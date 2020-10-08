using Newtonsoft.Json;
using System;
using DigWex.Extensions;
using DigWex.Helpers;
using System.IO;
using DigWex.Api.Model;
using DigWex.Model;

namespace DigWex
{
  public sealed class Config
  {
    private const string SECRET = "fgsljgg-hn-vv";

    private static readonly Lazy<Config> _instance =
        new Lazy<Config>(() => new Config());

    private static string _content;
    private static string _serverUrl;
    private static string _contentUrl;
    public static EffectParams FadeEffect;
    public static bool _sync;
    public static bool _minimize;

    public static bool Demo { get; set; }

    public static string ContentUrl => _contentUrl;

    public static string RootDrive { get; private set; }

    public static string ContentDrive { get; private set; }

    private Config()
    {
      //if (_settings.UpgradeRequire) {
      //  _settings.Upgrade();
      //  _settings.UpgradeRequire = false;
      //  _settings.Save();
      //}
      //_content = _settings.Content;
      //_updateUrl = _settings.UpdateUrl;
      //_serverUrl = _settings.ServerUrl;
      //_sync = _settings.SyncWait;
      //_activateData = _settings.Activate.ToJsonObject<ActivateModel.Response>();
      //FadeEffect = _settings.FadeEffect.ToJsonObject<EffectParams>()
      //  ?? new EffectParams();

      //MouseExpanding = _settings.MouseExpanding.ToJsonObject<MouseExpanding>()
      //  ?? new MouseExpanding();
      if (string.IsNullOrEmpty(_serverUrl)) {
#if ADVELIT
        _serverUrl = "http://player.advelit.com/";
#else
        _serverUrl = "http://digwex.com/";
#endif
      }
    }

    public static ScreenBound ScreenBound { get; set; }

    public static Config Instance => _instance.Value;

    public static string SignData { get; private set; }

    public static string SignMd5 { get; private set; }

    public static bool IsLoaded { get; private set; }

    public static string Pin { get; set; }

    public static long LastRebootId { get; set; }

    public static bool SyncWait {
      get => _sync;
      set => _sync = value;
    }

    public static string ContentDir {
      get => _content;
      set => _content = value;
    }

    public static string AppData => _appData;

    public static string ServerUrl {
      get => _serverUrl;
      set {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri res)) {
          if (res.PathAndQuery.Trim('/') != "") {
            res = new Uri(res.Scheme + Uri.SchemeDelimiter + res.Authority);
          }
          ServerUri = res;
          _serverUrl = res.AbsoluteUri; // use spec;
        }
      }
    }

    public static Uri ServerUri;

    public static ActivateModel.Response ActivateData {
      get => _activateData;
      set => _activateData = value;
    }

    public bool Init(ConfigWebsocketModel config)
    {
      byte[] secret = SECRET.ToByte();
      byte[] encrypt = CryptoHelper.SignData(secret);
      SignData = StringExtensions.ByteToString(encrypt);
      SignMd5 = CryptoHelper.Md5(SignData);

      ScreenBound = config.screen;

      Log.Info($"Sign device: {SignMd5}");
      _serverUrl = config.Config.ServerUrl;
      _activateData = config.Activate;
      LastRebootId = config.LastRebootId;
      _content = config.Config.ContentPath;
      _appData = config.Config.AppData.Trim(new char[] { '/', '\\' });

      if (_serverUrl.IndexOf("http://") == -1
        && _serverUrl.IndexOf("https://") == -1) {
        _serverUrl = "http://" + _serverUrl;
      }

      if (Uri.TryCreate(_serverUrl, UriKind.Absolute, out Uri res)) {
        if (res.PathAndQuery.Trim('/') != "") {
          res = new Uri(res.Scheme + Uri.SchemeDelimiter + res.Authority);
          ServerUrl = res.AbsoluteUri;
        }
        ServerUri = res;
        _serverUrl = res.AbsoluteUri; // use spec;
      }
      else {
        return false;
      }

      _contentUrl = new Uri(_content) + "";

      //RootDrive = Program.Directory[0] + "";
      //ContentDrive = _content[0] + "";
      return true;
    }

    private static ActivateModel.Response _activateData;
    private static string _appData;
  }

  public class EffectParams : ICloneable
  {
    public bool Enable;

    public int In;

    public int Out;

    public object Clone()
    {
      return MemberwiseClone();
    }
  }

}