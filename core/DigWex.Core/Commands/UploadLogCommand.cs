using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace DigWex.Commands
{
  public class UploadLogCommand : Command
  {
    public async override void StartAsync()
    {
      Log.Info($"Start upload-logs command, id: {CommandId}");
      bool ok = await Task.Run(() => UploadLog());
      //CommandService.RemovePriorityCommand(typeof(UploadLogsCommandModel));
      if (ok)
        Log.Info($"Complete upload-logs command, id: {CommandId}");
      else
        Log.Info($"Error upload-logs command, id: {CommandId}");
    }

    private bool UploadLog()
    {
      string sourceDir = Path.GetFullPath(Program.Directory + "/logs");

      try {
        using (var stream = new MemoryStream()) {
          using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8)) {
            foreach (string filePath in Directory.GetFiles(sourceDir)) {
              using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
              using (Stream fileStreamInZip = archive.CreateEntry(Path.GetFileName(filePath)).Open())
                fileStream.CopyTo(fileStreamInZip);
            }
          }
          stream.Position = 0;
          Api.DeviceApi.Instance.UploadLog(stream);
          return true;
        }
      }
      catch { }
      return false;
    }
  }
}
