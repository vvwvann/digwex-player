using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DigWex.Api.Model;
using DigWex.Extensions;
using DigWex.Utils;
using static DigWex.Api.Model.PackageModel;

namespace DigWex.Managers
{
  public abstract class PackageManagerBase : IPackageManager
  {
    public event EventHandler<bool> UpdatedPlaylist = (e, v) => { };

    protected volatile Dictionary<int, MediaPlaylistBase> _playlists = new Dictionary<int, MediaPlaylistBase>();

    protected DataContext _dataContext = DataContext.Instance;
    protected int _packageId = -1;

    public int PackageId => _packageId;

    public Dictionary<int, MediaPlaylistBase> MediaPlaylists => _playlists;

    public abstract Task UpdatePackageAsync(PackageModel model);

    public abstract Task UpdateBase(DataModel[] datas);

    protected virtual void OnUpdatePlaylist(bool force)
    {
      Console.WriteLine("UPDATE PLAYLIST");
      UpdatedPlaylist(this, force);
    }

    public virtual async Task InitAsync()
    {
      await Task.Run(async () => {

        var security = new Security();
        string path = Path.GetFullPath(Config.AppData + '/' + AppConst.DATA_PACKAGE_FILE);
        PackageModel model = await JsonExtensions.LoadJsonFromFileAsync<PackageModel>(path);
        if (model == null) return;

        var used = new HashSet<string>();

        ContentModel[] contents = model.Contents ?? new ContentModel[0];
        foreach (var content in contents) {
          if (!await CheckExistAndSetPath(content)) {
            Console.WriteLine(content.Url);
            Console.WriteLine("AAAAAAAAAAAAAAAA");
            return;
          }
          if (content.Path != null && content.Type != AppConst.HTML_TYPE)
            used.Add(content.Path.LocalPath);
        }

        Dfs(Path.GetFullPath(Config.ContentDir), used, DateTime.Now);
        await UpdatePackageAsync(model);

        return;
      });
    }

    private void Dfs(string path, HashSet<string> used, DateTime now)
    {
      string[] dirs = null;
      FileInfo[] files = null;
      try {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        string name = directoryInfo.Name.ToLower();
        if (name == "source" || name == "html") return;
        files = directoryInfo.GetFiles();
        dirs = Directory.GetDirectories(path);
      }
      catch {
        return;
      }

      if (files != null) {
        foreach (FileInfo file in files) {
          //Console.WriteLine(file.FullName + " - " + removes.Contains(file.FullName));
          if (now.Subtract(file.LastAccessTime).Days > 7 && !used.Contains(file.FullName)) {
            try {
              file.Delete();
            }
            catch { continue; }
          }
        }
      }

      if (dirs == null) return;

      foreach (string p in dirs)
        Dfs(p, used, now);
    }

    private async Task<bool> CheckExistAndSetPath(ContentModel content)
    {
      if (content.Type == AppConst.URL) return true;

      string first = $"{content.Md5[0]}{content.Md5[1]}";
      string second = $"{content.Md5[2]}{content.Md5[3]}";
       
      string url = $"{Config.ContentDir}/{AppConst.FILES_DIR}/{first}/{second}/{content.Local.Replace("\\", "/")}";
      if (!await FileUtils.FileExistAsync(Path.GetFullPath(url))) return false;
      url = $"{Config.ContentUrl}/{AppConst.FILES_DIR}/{first}/{second}/{content.Local.Replace("\\", "/")}";

      content.Path = new Uri(url, UriKind.Absolute);
      return true;
    }
  }
}