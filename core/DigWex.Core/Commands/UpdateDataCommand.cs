using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigWex.Helpers;
using DigWex.Managers;
using DigWex.Model;
using DigWex.Model.Commands;
using System.IO;

namespace DigWex.Commands
{
  public class UpdateDataCommand : Command
  {
    private List<JToken> _jtokens = new List<JToken>();
    private readonly DefaultPackageManager _packageService;
    private List<KeyValuePair<Uri, string>> _files = new List<KeyValuePair<Uri, string>>();
    private readonly UpdataDataKwargs _kwargs;

    public UpdateDataCommand(UpdataDataCommandModel model) : base(model.Id)
    {
      _kwargs = model.Kwargs;
      _packageService = (DefaultPackageManager)PackageManager.Instance;
    }

    public async Task DownloadItems(UpdataDataKwargs data)
    {
      PreDfs(ref data._data);
      DownloadManager manager = new DownloadManager();
      string folder = Path.GetFullPath(Config.ContentDir
              + '/' + AppConst.SOURCE_DIR);
      foreach (var item in _files)
      {
        await manager.AddFileAsync(item.Key.AbsolutePath, folder, item.Value);
      }
      Console.WriteLine("Wait");
      await manager.StartAsync();
    }

    public List<KeyValuePair<Uri, string>> GetFiles(UpdataDataKwargs[] data)
    {
      if (data == null) return _files;

      foreach (UpdataDataKwargs item in data)
      {
        PreDfs(ref item._data);
      }

      return _files;
    }

    public override async void StartAsync()
    {
      Log.Info($"Start update-date command, id: {CommandId}");

      if (_kwargs == null)
      {
        Log.Info($"End update-date command, id: {CommandId}");
        return;
      }

      await DownloadItems(_kwargs);

      //_packageService.UpdateDataAsync(_kwargs);

      //CommandService.RemovePriorityCommand(typeof(UpdataDataCommandModel));

      Log.Info($"End update-date command, id: {CommandId}");
    }

    private void PreDfs(ref JToken data)
    {
      var obj = new JObject
      {
        ["d"] = data
      };

      Dfs(obj);
      data = obj["d"];
    }

    private void Dfs(JToken token)
    {
      if (token is JObject obj)
      {
        bool ok = obj.TryGetValue("$playback-data-resource", out JToken res);
        if (ok && res.Type == JTokenType.String
            && token.Parent is JProperty prop)
        {
          string url = (string)res;
          int index = url.LastIndexOf('/');
          if (index == -1) return;
          string local = url.Substring(index + 1);
          try
          {
            _files.Add(new KeyValuePair<Uri, string>(new Uri(url), local));
            prop.Value = Config.ContentUrl + '/'
                + AppConst.SOURCE_DIR + '/'
                + local;
          }
          catch { }
          return;
        }
      }

      JEnumerable<JToken> tokens = token.Children();
      foreach (JToken item in tokens)
        Dfs(item);
    }
  }
}
