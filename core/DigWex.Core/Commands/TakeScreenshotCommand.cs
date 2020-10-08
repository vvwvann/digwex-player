using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using DigWex.Model.Commands;

namespace DigWex.Commands
{
  class TakeScreenshotCommand : Command
  {
    public override async void StartAsync()
    {
      Log.Info($"Start take-screenshot command, id: {CommandId}");
      bool ok = await Task.Run(() => TakeScreenshot());
      //CommandService.RemovePriorityCommand(typeof(TakeScreenshotCommandModel));
      if (ok)
        Log.Info($"Complete take-screenshot command, id: {CommandId}");
      else
        Log.Error($"Error take-screenshot command, id: {CommandId}");
    }

    public bool TakeScreenshot()
    {
      //Bitmap bmp = null;
      //Graphics graphics = null;
      //MemoryStream stream = null;
      //try {
      //  ScreenRect(out int x, out int y, out int width, out int height);

      //  bmp = new Bitmap(width, height);
      //  graphics = Graphics.FromImage(bmp);
      //  graphics.CopyFromScreen(x, y, 0, 0, bmp.Size);
      //  stream = new MemoryStream();
      //  bmp.Save(stream, ImageFormat.Jpeg);
      //  stream.Position = 0;
      //  Api.DeviceApi.Instance.UploadScreenshot(stream);
      //  return true;
      //}
      //catch { }
      //finally {
      //  bmp?.Dispose();
      //  graphics?.Dispose();
      //  stream?.Dispose();
      //}
      return true;
    }

    //private void ScreenRect(out int x, out int y, out int width, out int height)
    //{
    //  x = 0;
    //  y = 0;
    //  width = (int)SystemParameters.PrimaryScreenWidth;
    //  height = (int)SystemParameters.PrimaryScreenHeight;

    //  System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

    //  int pX = 0;
    //  int pY = 0;
    //  App.Window.Dispatcher.Invoke(() => {
    //    pX = (int)App.Window.Left;
    //    pY = (int)App.Window.Top;
    //  });

    //  pX++;
    //  pY++;

    //  foreach (var item in screens) {
    //    int lX = item.Bounds.X;
    //    int rX = item.Bounds.X + item.Bounds.Width;
    //    int lY = item.Bounds.Y;
    //    int rY = item.Bounds.Y + item.Bounds.Height;

    //    if (pX >= lX && pX <= rX && pY >= lY && pY <= rY) {
    //      width = item.Bounds.Width;
    //      height = item.Bounds.Height;
    //      x = item.Bounds.X;
    //      y = item.Bounds.Y;
    //      return;
    //    }
    //  }
    //}
  }
}
