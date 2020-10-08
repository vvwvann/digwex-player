using System;
using System.IO;
using System.Threading.Tasks;

namespace DigWex.Utils
{
  public static class FileUtils
  {
    public static Task DeleteAsync(string filePath)
    {
      return Task.Factory.StartNew(() => {
        File.Delete(filePath);
      });
    }

    public static Task<bool> FileExistAsync(string filePath)
    {
      return Task.Factory.StartNew(() => {
        return File.Exists(filePath);
      });
    }

    public static Task<bool> MoveAsync(string sourceFileName, string destFilename)
    {
      return Task.Factory.StartNew(() => {
        try {
          File.Move(sourceFileName, destFilename);
          return true;
        }
        catch {
          return false;
        }
      });
    }

    public static string GetTmpFileEx(string path)
    {
      int st = path.LastIndexOf(".");
      if (st == -1) return null;
      path = path.Remove(st);
      path += ".tmp";
      return path;
    }

    public static Task<bool> ReplaceOrCreateAsync(string sourceFileName, string destFilename)
    {
      return Task.Factory.StartNew(() => {
        try {
          if (!File.Exists(destFilename))
            return MoveAsync(sourceFileName, destFilename).Result;
          File.Replace(sourceFileName, destFilename, null);
          return true;
        }
        catch {
          return false;
        }
      });
    }

    public static bool ExistOrCreate(string url)
    {
      var info = new DirectoryInfo(url);
      if (!info.Exists) {
        try {
          info.Create();
        }
        catch {
          return false;
        };
      }
      return true;
    }

    public static bool TryExistOrCreate(string path)
    {
      try {
        var info = new DirectoryInfo(path);
        if (!info.Exists) info.Create();
      }
      catch {
        return false;
      };
      return true;
    }
  }
}
