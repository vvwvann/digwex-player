using System;
using System.Threading.Tasks;
using DigWex.Managers;
using DigWex.Manager;
using DigWex.Services;
using DigWex.Api;
using DigWex.Network;
using DigWex.Utils;
using System.IO;
using DigWex.Extensions;

namespace DigWex
{
  public static class App
  {
    private static HttpServer _http = HttpServer.Instance;

    public static void Run(int port)
    {
      _http.OnConfig += OnConfig;
      _http.Connect(port);
    }

    private static async void OnConfig(object sender, EventArgs e)
    {
      bool ok = FileUtils.TryExistOrCreate(Path.GetFullPath(Config.AppData + "/databases"))
        && FileUtils.TryExistOrCreate(Path.GetFullPath(Config.ContentDir))
        && FileUtils.TryExistOrCreate(Path.GetFullPath(Config.ContentDir + '/' + AppConst.SOURCE_DIR))
        && FileUtils.TryExistOrCreate(Path.GetFullPath(Config.ContentDir + '/' + AppConst.FILES_DIR));

      if (!ok) return;


      DateTimeService.Instance.Start();

      try {
        await DataContext.Instance.Init();
      }
      catch {
        Log.Error("Open database problem");
        Program.ExitApp(0);
      }


      PackageManager.Init();
      SocketManager.Instance.StartLonger();

      try {
        await PackageManager.Instance.InitAsync();
      }
      catch (Exception ex) {
        Log.Error(ex, "Can't initialize package");
      }

      if (await UnpairAsync())
        return;

      if (Config.SyncWait || PackageManager.Instance.PackageId == -1)
        CommandManager.Instance.StartSyncCommand();

      StatsService.Instance.Start();
      DefaultPlaylistManager pl = new DefaultPlaylistManager();
      pl.Start();
    }

    public static async Task<bool> UnpairAsync()
    {
      bool ok = await DeviceApi.Instance.TokenAsync();

      if (ok) return false;


      Log.Info("Unpair player");

      Deactivate();
      return true;
    }

    private async static void Deactivate()
    {
      Log.Info("Deactivate");
      await HttpServer.Instance.SendCommandAsync(new {deactivate = true }.ToJsonString());
      Program.ExitApp(-1);
    }

  }
}