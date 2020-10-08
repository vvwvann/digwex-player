using DigWex;
using DigWex.Extensions;
using DigWex.Helpers;
using DigWex.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DigWex.Helpers
{
  public class DownloadManager
  {
    public event EventHandler<ErrorDownloadEventArgs> ErrorDownload = (e, v) => { };
    public event EventHandler<ProgressDownloadEventArgs> ProgressDownload = (e, v) => { };
    public event EventHandler<DownloadFile> CompleteDownloadFile = (e, v) => { };

    public const int ERROR_EXCEEDED_BYTES = 0;
    public const int ERROR_MD5_COMPARE = 1;

    private const int ATTEMPT_COUNT = 5;

    private readonly object _locker = new object();
    private readonly HashSet<DownloadFile> _downloadFiles = new HashSet<DownloadFile>();
    private readonly HashSet<DownloadFile> _completeFiles = new HashSet<DownloadFile>();
    private readonly ConcurrentDictionary<DownloadFile, bool> _downloadQueue = new ConcurrentDictionary<DownloadFile, bool>();
    private readonly string _authorization;

    private long _totalSize;
    private long _currSize;
    private int _prevPercent = -1;
    private bool _queueStart;

    public DownloadManager()
    {
      _authorization = "Bearer " + Config.ActivateData?.Token;
    }

    public async Task AddFileAsync(string url, string folder, string fileName) // folder have backslash format
    {
      string fullName = Path.GetFullPath(folder + '/' + fileName);
      FileInfo info = null;
      try {
        info = new FileInfo(fullName);
        if (info.Exists) return;
      }
      catch { }

      long length = await GetFileLength(url);
      if (length == -1) return;
      string type = ".";
      int index = fileName.LastIndexOf('.');
      if (index != -1) {
        type = fileName.Substring(index);
        fileName = fileName.Substring(0, index);
      }
      var file = new DownloadFile(url, folder, fileName, type, length);

      if (_completeFiles.Contains(file)
        || _downloadFiles.Contains(file)) return;

      long currSize = -1;
      try {
        currSize = await CheckFileTmp(file);
      }
      catch { return; }
      if (currSize == length) {
        _completeFiles.Add(file);
        return;
      }
      file.StartByte = currSize;
      _currSize += currSize;
      _totalSize += file.Size;
      _downloadFiles.Add(file);
    }

    public async Task AddFileAsync(DownloadFile file) // 1 added, 2 exist, -1 - error, 0 - download other
    {
      if (_completeFiles.Contains(file)
        || _downloadFiles.Contains(file)) return;

      FileInfo info = null;
      try {
        info = new FileInfo(file.FullName);
        if (info.Exists) return;
      }
      catch { }

      long currSize = -1;
      try {
        currSize = await CheckFileTmp(file);
      }
      catch { return; }
      if (currSize == file.Size) {
        _completeFiles.Add(file);
        return;
      }
      file.StartByte = currSize;
      _currSize += currSize;
      _totalSize += file.Size;
      _downloadFiles.Add(file);
    }

    public async Task AddFileAsync(DownloadMd5File file)
    {
      if (file == null
        || _completeFiles.Contains(file)
        || _downloadFiles.Contains(file)) return;
      try {
        bool ok = await CheckMd5File(file);  //exception
        if (ok) {
          _completeFiles.Add(file);
          return;
        }
        long currSize = await CheckMd5FileTmp(file);
        if (currSize == file.Size) {
          _completeFiles.Add(file);
          return;
        }
        file.StartByte = currSize;
        _currSize += currSize;
      }
      catch { }
      _totalSize += file.Size;
      _downloadFiles.Add(file);
    }

    public void EnqueueToDownload(DownloadFile file)
    {
      Task.Run(async () => {
        if (_downloadQueue.ContainsKey(file)) return;

        FileInfo info = null;
        try {
          info = new FileInfo(file.FullName);
          if (info.Exists) {
            CompleteDownloadFile(this, file);
            return;
          }
        }
        catch { }

        long currSize = -1;
        try {
          currSize = await CheckFileTmp(file);
        }
        catch { }
        if (currSize == file.Size) {
          CompleteDownloadFile(this, file);
          return;
        }
        file.StartByte = currSize;
        _downloadQueue[file] = true;

        await StartQueueAsync();
      });
    }

    private async Task StartQueueAsync()
    {
      lock (_locker) {
        if (_queueStart) return;
        _queueStart = true;
      }

      //Console.WriteLine("QUEUE DOWNLOAD");

      foreach (var item in _downloadQueue) {
        DownloadFile file = item.Key;
        if (await DownloadAndResumingAsync(file)) {
          CompleteDownloadFile(this, file);
        }
        else {
          _downloadFiles.Remove(file);
        }
        _downloadQueue.TryRemove(file, out bool ok);
      }

      lock (_locker) {
        _queueStart = false;
      }
    }

    public async Task<bool> StartAsync(bool ignore = true)
    {
      bool ok = true;
      foreach (var file in _downloadFiles) {
        long tmp = _currSize;
        bool complete = false;

        if (file is DownloadMd5File md5File) {
          complete = await DownloadAndResumingAsync(md5File);
          if (complete && file.Type == AppConst.ZIP)
            ok &= Unzip(file.FullName, Path.GetFullPath(file.Folder + '/' + md5File.Md5));
        }
        else {
          complete = ignore | await DownloadAndResumingAsync(file);
        }
        ok &= complete;
      }
      return ok;
    }

    public async Task<byte[]> DownloadToByteAsync(string url)
    {
      var data = new List<byte>();
      try {
        var req = (HttpWebRequest)WebRequest.Create(url);
        req.Timeout = 5000;
        using (var webResponse = (HttpWebResponse)await req.GetResponseAsync()) {
          byte[] buffer = new byte[4096];
          using (Stream input = webResponse.GetResponseStream()) {
            int size;
            while ((size = await input.ReadAsync(buffer, 0, buffer.Length)) > 0) {
              _currSize += size;
              for (int i = 0; i < size; i++)
                data.Add(buffer[i]);
            }
          }
        }
      }
      catch { }
      return data.ToArray();
    }

    private async Task<long> GetFileLength(string url)
    {
      long length = -1;
      for (int i = 0; length == -1 && i < 5; i++) {
        try {
          var response = await VvHelpers.HttpMethods.HeadAsync(url);
          if (response != null && response.StatusCode == HttpStatusCode.OK) {
            length = response.ContentLength;
            if (length > 0) return length;
            length = -1;
          }
        }
        catch (Exception ex) {
          Console.WriteLine("Exception get info file: {0}", ex.Message);
        }
        await Task.Delay(2000);
      }
      return length;
    }

    private async Task<bool> CheckMd5File(DownloadMd5File file)
    {
      var info = new FileInfo(file.FullName);
      if (info.Exists) {
        long length = info.Length;
        if (file.Size == length) {
          string fileMd5 = CryptoHelper.Md5File(file.FullName);
          if (file.Md5 == fileMd5) return true;
        }
        await info.DeleteAsync();
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="SystemException"></exception>
    /// <returns></returns>
    private async Task<long> CheckMd5FileTmp(DownloadMd5File file)
    {
      var info = new FileInfo(file.TmpPath);
      if (info.Exists) {
        long length = info.Length;
        if (file.Size == length) {
          string fileMd5 = CryptoHelper.Md5File(file.TmpPath);
          if (file.Md5 == fileMd5
              && await FileUtils.MoveAsync(file.TmpPath, file.FullName))
            return length;
          length++;
        }
        if (file.Size < length)
          await info.DeleteAsync();
        else return length;
      }
      return 0;
    }

    private async Task<long> CheckFileTmp(DownloadFile file)
    {
      var info = new FileInfo(file.TmpPath);
      if (info.Exists) {
        long length = info.Length;
        if (length == file.Size) {
          if (await FileUtils.MoveAsync(file.TmpPath, file.FullName)) return length;
          length++;
        }
        if (file.Size < length)
          await info.DeleteAsync();
        return length;
      }
      return 0;
    }

    private async Task<bool> DownloadAndResumingAsync(DownloadFile file) =>
      await DownloadFileAsync(file, file.TmpPath) &&
        await FileUtils.MoveAsync(file.TmpPath, file.FullName);

    private async Task<bool> DownloadAndResumingAsync(DownloadMd5File file)
    {
      bool ok = await DownloadFileAsync(file, file.TmpPath);
      if (ok) {
        string md5 = CryptoHelper.Md5File(file.TmpPath);
        if (file.Md5 == md5
            && await FileUtils.MoveAsync(file.TmpPath, file.FullName))
          return true;
        Console.WriteLine(new { md5, file.Md5 });
        ErrorDownload(this, new ErrorDownloadEventArgs(ERROR_MD5_COMPARE, file, _currSize, _totalSize, file.Size));
        _currSize -= file.Size;
        await FileUtils.DeleteAsync(file.TmpPath);
      }
      return false;
    }

    private void ProgressDownloadEvent(DownloadFile file, long startByte)
    {
      int percent = (int)(_currSize / (double)_totalSize * 100);
      if (_prevPercent != percent) {
        _prevPercent = percent;
        ProgressDownload(null, new ProgressDownloadEventArgs(file, _currSize, _totalSize, startByte));
      }
    }

    private bool Unzip(string fileName, string folder)
    {
      try {
        ZipUtils.Unzip(fileName, folder);
        File.Delete(fileName);
      }
      catch (Exception ex) {
        Log.Warn(ex, $"Extract files from {fileName}");
        return false;
      }
      return true;
    }

    private async Task<bool> DownloadFileAsync(DownloadFile file, string filePath)
    {
      int attempt = 0;
      long startByte = file.StartByte;
      Console.WriteLine(new { startByte });
      while (startByte < file.Size && ++attempt < ATTEMPT_COUNT) {
        ProgressDownloadEvent(file, startByte);
        FileStream fileStream = null;
        HttpWebResponse webResponse = null;
        Stream input = null;
        try {
          var req = (HttpWebRequest)WebRequest.Create(file.Url);
          req.Timeout = 10000;
          req.Headers[HttpRequestHeader.Authorization] = _authorization;
          req.AddRange(startByte);
          webResponse = await req.GetResponseTimeoutAsync();
          byte[] buffer = new byte[4096];
          fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
          input = webResponse.GetResponseStream();

          int size;

          while ((size = await input.ReadTimeoutAsync(buffer, 0, buffer.Length, 10000)) > 0) {

            await fileStream.WriteAsync(buffer, 0, size);
            _currSize += size;
            startByte += size;

            ProgressDownloadEvent(file, startByte);
          }
        }
        catch(Exception ex) {
          Console.WriteLine(ex);
          await Task.Delay(5000);
        }
        finally {
          fileStream?.Dispose();
          webResponse?.Dispose();
          input?.Dispose();
        }
      }

      if (startByte > file.Size) {
        _currSize -= startByte;
        Console.WriteLine(new {startByte, file.Size });
        ErrorDownload(this, new ErrorDownloadEventArgs(ERROR_EXCEEDED_BYTES, file, _currSize, _totalSize, startByte));
      }

      return startByte == file.Size;
    }
  }

  public class DownloadFile
  {
    private readonly string _tmpPath;
    private readonly string _url;
    private readonly string _folder;
    private readonly string _fileName;
    private readonly string _fullName;
    private readonly string _type;
    protected readonly long _size;

    public DownloadFile(string url, string folder, string name, string type, long size)
    {
      _url = url;
      _size = size;
      _type = type;
      _folder = folder;
      _fileName = name + type;
      _fullName = Path.GetFullPath(folder + '/' + name);
      _tmpPath = _fullName + AppConst.TMP;
      _fullName += type;
    }

    public string TmpPath => _tmpPath;

    public string FullName => _fullName;

    public string Url => _url;

    public string Folder => _folder;

    public string FileName => _fileName;

    public long Size => _size;

    public string Type => _type;

    public long StartByte { get; set; }

    public override int GetHashCode()
    {
      return FileName.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is DownloadFile tmp)
        return _size == tmp._size
          && _fullName == tmp._fullName;
      return false;
    }
  }

  public class DownloadMd5File : DownloadFile
  {
    private readonly string _md5;

    public DownloadMd5File(string url, string folder, string name, string md5, string type, long size)
      : base(url, folder, name, type, size)
    {
      _md5 = md5;
    }

    public string Md5 => _md5;

    public override int GetHashCode()
    {
      return _md5.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return (obj is DownloadMd5File tmp) && _size == tmp._size
        && _md5 == tmp._md5;
    }
  }

  public abstract class DownloadEventArgs : EventArgs
  {
    public DownloadEventArgs(DownloadFile file, long currentBytes, long totalBytes, long currentFileBytes)
    {
      File = file;
      CurrentBytes = currentBytes;
      TotalBytes = totalBytes;
      CurrentFileBytes = currentFileBytes;
    }

    public DownloadFile File { get; set; }

    public long CurrentBytes { get; }

    public long TotalBytes { get; }

    public long CurrentFileBytes { get; }
  }

  public class ErrorDownloadEventArgs : DownloadEventArgs
  {
    public ErrorDownloadEventArgs(int type, DownloadFile file, long currentBytes, long totalBytes, long currentFileBytes) :
        base(file, currentBytes, totalBytes, currentFileBytes)
    {
      Type = type;
    }

    public int Type { get; }
  }

  public class ProgressDownloadEventArgs : DownloadEventArgs
  {
    public ProgressDownloadEventArgs(DownloadFile file, long currentBytes, long totalBytes, long currentFileBytes) :
        base(file, currentBytes, totalBytes, currentFileBytes)
    {
    }
  }
}