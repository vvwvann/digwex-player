using System;
using System.Threading.Tasks;
using DigWex.Helpers;
using DigWex.Managers;
using System.Net;
using DigWex.Api;
using DigWex.Api.Model;
using DigWex.Extensions;
using Newtonsoft.Json.Linq;
using static DigWex.Api.Model.PackageModel;
using System.IO;
using DigWex.Utils;

namespace DigWex.Commands
{
  public class SynchronizeCommand : Command
  {
    private const int INFINITY_SYNC = 20;

    private readonly DefaultPackageManager _packageDataService =
      (DefaultPackageManager)PackageManager.Instance;
    private string _nameFile = AppConst.DATA_PACKAGE_FILE;
    private readonly DeviceApi _deviceApi;

    private int prev = -1;

    public SynchronizeCommand(int id = -1) : base(id)
    {
      _deviceApi = DeviceApi.Instance;
    }

    public async void StarDemoAsync()
    {
      _nameFile = "data-demo";
      string path = Path.GetFullPath($"{Config.AppData}/bin/{_nameFile}.json");
      PackageModel model = await JsonExtensions.LoadJsonFromFileAsync<PackageModel>(path);

      if (model == null) {

        // model = Properties.Resources.demoPackage.ToJsonObject<PackageModel>();
        if (model == null) {
          Log.Info("Failed to create demo data");
          return;
        }

        bool ok = await model.JsonToFileAsync(path, Newtonsoft.Json.Formatting.Indented);
        if (ok) {
          Log.Info("Demo data created successfully");
        }
        else {
          Log.Info("Failed to create demo data");
        }
      }

      await SyncAsync(model);

      try {
        JToken data = _packageDataService.GetValuesData(467)?.Data;
        if (data == null) return;

        data = data["list"];

        DateTime now = DateTime.UtcNow.Date;

        JEnumerable<JToken> children = data.Children();
        foreach (var item in children) {
          item["dt"] = now.UnixTime();
          now = now.AddHours(3);
        }
      }
      catch { }
    }

    public override async void StartAsync()
    {
      Config.SyncWait = true;

      Log.Info("Start synchronize command id " + CommandId);
      int attemp = 0;
      while (true) {
        ResultPack result = await _deviceApi.DataAsync();

        if (result.Code == HttpStatusCode.Unauthorized) {
          Log.Warn($"Player not authorized can not receive data package, command {CommandId} can not be continued");
          CommandService.RemovePriorityCommand(1);
          return;
        }
        if (result.Code == HttpStatusCode.OK) {
          PackageModel model = result.Model;
          if (model != null) {
            try {
              bool ok = await SyncAsync(model);
              if (ok) {
                Config.SyncWait = false;
                break;
              }
            }
            catch (Exception ex) {
              Log.Warn($"During package {CommandId} synchronization, an error occurred {ex}");
            }
            if (attemp == INFINITY_SYNC) {
              Log.Warn("Too many attempts to synchronize command");
              attemp = 0;
            }
            await Task.Delay(10000);
          }
          else {
            Log.Warn($"Package data does not match the API model, command {CommandId}");
          }
        }
        else {
          Log.Warn($"During package {CommandId} synchronization, an error occurred {result.Code}");
        }
        attemp++;

        await Task.Delay(20000);
      }

      //var item = new JournalItem(JournalTypes.SyncProgress) {
      //  Data = new SyncProgressModel {
      //    Id = model.Id
      //  }
      //};

      //Journal.AddItem(item);
      CommandService.RemovePriorityCommand(1);
      Journal.SetSyncInfo(null, 100);
      Log.Info($"Success synchronize command id {CommandId}");
    }

    private async Task<bool> SyncAsync(PackageModel model)
    {
      Console.WriteLine("PackageId: " + model.Id);
      var manager = new DownloadManager();
      var contents = model.Contents ?? new ContentModel[0];
      var datas = model.Data ?? new DataModel[0];

      await AddMainFiles(manager, contents);

      manager.ProgressDownload += ProgressDownload;
      manager.ErrorDownload += ErrorDownload;

      bool ok = await manager.StartAsync();
      if (!ok) {
        Log.Info($"Failed downloading files of package id {model.Id}");
        return false;
      }

      Log.Info($"Complete downloading files of package id {model.Id}");
      if (model.Data != null)
        await _packageDataService.UpdateBase(datas);

      //if (model.Id == _packageDataService.PackageId) return true;


      ok = await model.JsonToFileAsync(Path.GetFullPath($"{Config.AppData}/{_nameFile}"));

      if (ok) {
        Log.Info("Data package written to file");
        await _packageDataService.UpdatePackageAsync(model);
        return true;
      }

      Log.Warn("Failed to write the data packet in the file system");
      return false;
    }

    private async Task AddMainFiles(DownloadManager manager, ContentModel[] contents)
    {
      foreach (ContentModel content in contents) {
        await AddFile(manager, content);
        if (content.Type != AppConst.URL) {
          content.Url = content.Local;
        }
      }
    }

    private async Task AddFile(DownloadManager manager, ContentModel file)
    {
      string ext = null;

      switch (file.Type) {
        case AppConst.HTML_ZIP_TYPE:
          ext = AppConst.ZIP;
          file.Type = AppConst.HTML_TYPE;
          break; 
        case AppConst.HTML_TYPE:
          ext = AppConst.HTML;
          break;
        case AppConst.URL_TYPE:
          return;
      }

      string first = $"{file.Md5[0]}{file.Md5[1]}";
      string second = $"{file.Md5[2]}{file.Md5[3]}";

      bool ok = FileUtils.TryExistOrCreate(Path.GetFullPath($"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}")) &&
        FileUtils.TryExistOrCreate(Path.GetFullPath($"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}/{second}"));
      if (!ok) return;


      var downloadFile = new DownloadMd5File(file.Url,
       Path.GetFullPath($"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}/{second}"),
       file.Md5,
       file.Md5,
       ext,
       file.Size);

      file.Local = file.Md5;// + ext;
      ok = true;

      if (ext == AppConst.ZIP) {
        string archive = Path.GetFullPath($"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}/{second}/{file.Md5}{AppConst.ZIP}");
        file.Local = file.Md5 + "/index";
        ok = File.Exists(archive) || !File.Exists(Path.GetFullPath($"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}/{second}/{file.Local}{AppConst.HTML}"));
      }
      string url = $"{Config.ContentUrl}/{AppConst.FILES_DIR}/{first}/{second}/{file.Local.Replace("\\", "/")}";
      if (file.Type == AppConst.HTML_TYPE) {
        url += AppConst.HTML;
        file.Local += AppConst.HTML;
      }
      file.Path = new Uri(url, UriKind.Absolute);

      if (ok) await manager.AddFileAsync(downloadFile);
    }
 
    private void ErrorDownload(object sender, ErrorDownloadEventArgs e)
    {
      string message = "Undefined error download file";
      switch (e.Type) {
        case DownloadManager.ERROR_MD5_COMPARE:
          message = $"MD5 are not equal for the file by url - {e.File.Url}, progress sync ({e.CurrentBytes} / {e.TotalBytes})";
          break;
        case DownloadManager.ERROR_EXCEEDED_BYTES:
          message = $"It was downloaded more bytes than the file size, local file - {e.File.FileName}, progress sync ({e.CurrentBytes} / {e.TotalBytes})";
          break;
      }
      Log.Warn(message);
    }

    private void ProgressDownload(object sender, ProgressDownloadEventArgs e)
    {
      float real = (float)(e.CurrentBytes / (double)e.TotalBytes);
      int percent = (int)(real * 100);

      //Journal.SetSyncInfo(new SynchronizationModel {
      //  DeviceDataId = Config.ActivateData.Id,
      //  Progress = real
      //}, percent);

      int curr = percent / 10;
      if (curr != prev) {
        prev = curr;
        Log.Info($"Synchronize progress: {percent}% ({e.CurrentBytes} / {e.TotalBytes})");
      }
    }
  }
}