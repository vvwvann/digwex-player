using DigWex.Api.Model;
using DigWex.Helpers;
using System;
using System.Threading.Tasks;

namespace DigWex.Commands
{
  public class SplashCommand : Command
  {
    public async override void StartAsync()
    {
      Log.Info($"Start splash command, id: {CommandId}");

      bool ok = await GetSplashAsync();

      if (ok)
        Log.Info($"Complete splash command, id: {CommandId}");
      else
        Log.Info($"Error splash command, id: {CommandId}");
    }

    private async Task<bool> GetSplashAsync()
    {
      try {
        SplashModel splash = await Api.DeviceApi.Instance.GetSplashAsync();

        //SplashModel splash = new SplashModel {
        //  Background = "#ff0000",
        //  Image = new SplashModel.Img {
        //    Size = 5425070,
        //    Md5 = "978c71ccb7a820d4a8defe0bf5c52a0c",
        //    Url = "https://webapi.advelit.com/external/api/v1/files/ab4adb9e9cecb03dea1072742335e8c493f75d5f"
        //  }
        //};

        if (splash == null) return false;

        DownloadManager dm = new DownloadManager();
        DownloadMd5File file = new DownloadMd5File(splash.Image.Url,
          $"{Config.ContentDir}//{AppConst.SOURCE_DIR}/",
          "splash",
          splash.Image.Md5,
          "",
          splash.Image.Size);

        await dm.AddFileAsync(file);
        await dm.StartAsync();

        //Config.Splash.Uri = $"{Config.ContentDir}\\{AppConst.SOURCE_DIR}\\splash";
        //Config.Splash.Background = splash.Background;

        //App.Window.Dispatcher.Invoke(() => {
        //  App.Window.SetSplashscreen();
        //});
        return true;
      }
      catch (Exception ex) { Console.WriteLine(ex); return false; }
    }



  }
}
