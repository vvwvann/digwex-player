using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigWex.Model.Commands;
using DigWex.Managers;
using System.IO;

namespace DigWex.Services
{
  public class ExternalService
  {
    private static ExternalService _instance = new ExternalService();

    private readonly Dictionary<string, ExternalClient> _clients =
        new Dictionary<string, ExternalClient>();

    public static ExternalService Instance {
      get {
        return _instance;
      }
    }

    public void Update(HashSet<string> used)
    {
      var removes = new List<string>();

      foreach (var item in _clients) {
        if (!used.Contains(item.Key))
          removes.Add(item.Key);
        else
          used.Remove(item.Key);
      }

      foreach (var item in removes) {
        _clients.TryGetValue(item, out ExternalClient client);
        _clients.Remove(item);
        client?.Stop();
      }

      foreach (var item in used) {
        var client = new ExternalClient(item);
        client.Start();
        _clients[item] = client;
      }
    }
  }

  public class ExternalClient
  {
    private readonly Uri _url;
    private readonly DefaultPackageManager _dataService;
    private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
    private NetworkStream _stream;
    private TcpClient _tcpClient;

    public ExternalClient(string url)
    {
      CancellationToken token = _cancelTokenSource.Token;
      _url = new Uri(url);
      _dataService = (DefaultPackageManager)PackageManager.Instance;
    }

    public void Start()
    {
      Task t = Task.Factory.StartNew(StartTask);
      //Log.Info("Start external client: " + _url);
    }

    private async void StartTask()
    {
      //Console.WriteLine("StartTask: {0}", Thread.CurrentThread.ManagedThreadId);
      while (!_cancelTokenSource.IsCancellationRequested) {
        try {
          ConnectToServer();
        }
        catch (Exception) {
          //Console.WriteLine("Exception external connect");
        }
        finally {
          if (_stream != null)
            _stream.Close();
          if (_tcpClient != null)
            _tcpClient.Close();
          await Task.Delay(3000);
        }
      }
    }

    private void ConnectToServer()
    {
      //Console.WriteLine("StartTask: {0}", Thread.CurrentThread.ManagedThreadId);
      string header = "GET " + _url.PathAndQuery + " HTTP/1.1\r\n"
          + "\r\n";

      byte[] buffer = new byte[1024];
      _tcpClient = new TcpClient();
      _tcpClient.Connect(_url.Host, _url.Port);

      _stream = _tcpClient.GetStream();
      _stream.ReadTimeout = 10000;
      _stream.WriteTimeout = 5000;
      byte[] bytesToSend = Encoding.ASCII.GetBytes(header);

      _stream.Write(bytesToSend, 0, bytesToSend.Length);
      _stream.Read(buffer, 0, buffer.Length);
      ReceiveMessage();
    }

    private void ReceiveMessage()
    {
      byte[] buffer = new byte[4096];
      while (!_cancelTokenSource.IsCancellationRequested) {
        try {
          int size = _stream.Read(buffer, 0, buffer.Length);
          string str = Encoding.ASCII.GetString(buffer, 0, size);

          string[] lines = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
          //int length = Convert.ToInt32(lines[0], 16);

          foreach (string line in lines) {
            string response = line.Replace("\n", "");
            _dataService.OnPushExternalPlaylist(response);
          }
        }
        catch (Exception) {
          //Console.WriteLine("WTF");
          return;
        }
      }
    }

    public void Stop()
    {
      _cancelTokenSource.Cancel();
      //Console.WriteLine("Stop external client: " + _url);
    }
  }

  public class DataService
  {
    private static DataService _instance = new DataService();

    private readonly Dictionary<string, DataClient> _clients =
        new Dictionary<string, DataClient>();

    public static DataService Instance {
      get {
        return _instance;
      }
    }

    public void Update(HashSet<string> used)
    {
      var removes = new List<string>();

      foreach (var item in _clients) {
        if (!used.Contains(item.Key))
          removes.Add(item.Key);
        else
          used.Remove(item.Key);
      }

      foreach (var item in removes) {
        _clients.TryGetValue(item, out DataClient client);
        _clients.Remove(item);
        client?.Stop();
      }

      foreach (var item in used) {
        var client = new DataClient(item);
        client.Start();
        _clients[item] = client;
      }
    }
  }

  public class DataClient
  {
    private readonly Uri _url;
    private readonly DefaultPackageManager _dataService;
    private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
    private NetworkStream _stream;
    private TcpClient _tcpClient;

    public DataClient(string url)
    {
      CancellationToken token = _cancelTokenSource.Token;
      _url = new Uri(url);
      _dataService = (DefaultPackageManager)PackageManager.Instance;
    }

    public void Start()
    {
      Task t = Task.Factory.StartNew(StartTask);
      //Log.Info("Start stream client: " + _url);
    }

    private async void StartTask()
    {
      //Console.WriteLine("StartTask: {0}", Thread.CurrentThread.ManagedThreadId);
      while (!_cancelTokenSource.IsCancellationRequested) {
        try {
          ConnectToServer();
        }
        catch {
          //Log.Warn(ex, "Push data problem");
        }
        finally {
          if (_stream != null)
            _stream.Close();
          if (_tcpClient != null)
            _tcpClient.Close();
          await Task.Delay(3000);
        }
      }
    }

    private void ConnectToServer()
    {
      string header = "GET " + _url.PathAndQuery + " HTTP/1.1\r\n"
          + "\r\n";

      _tcpClient = new TcpClient();
      _tcpClient.Connect(_url.Host, _url.Port);

      _stream = _tcpClient.GetStream();
      _stream.WriteTimeout = 5000;
      byte[] bytesToSend = Encoding.UTF8.GetBytes(header);

      _stream.Write(bytesToSend, 0, bytesToSend.Length);
      ReceiveMessage();
    }

    private void ReceiveMessage()
    {
      var streamReader = new StreamReader(_stream, Encoding.UTF8, true, 4096, true);

      string line = "";
      while (true) {
        line = streamReader.ReadLine();
        if (line.Length == 0) break;
        //Console.WriteLine(line);
      }

      //Console.WriteLine("END");

      string hex = streamReader.ReadLine();

      //Console.WriteLine("HEX: " + hex);
      int contentLength = Convert.ToInt32(hex, 16);

      if (contentLength == 0) return;

      var builder = new StringBuilder();

      while (!_cancelTokenSource.IsCancellationRequested && contentLength > -1) {
        if (contentLength == 0) {

          UpdateData(builder);
          builder.Clear();
          streamReader.ReadLine();

          hex = streamReader.ReadLine();

          //Console.WriteLine("HEX: " + hex);

          contentLength = Convert.ToInt32(hex, 16);
        }

        line = streamReader.ReadLine();
        //Console.WriteLine("LINE: " + line);
        builder.Append(line);
        contentLength -= line.Length;
        contentLength -= 1;

        //Console.WriteLine("CONTENT_LENGTH: " + contentLength);
      }
    }

    private void UpdateData(StringBuilder builder)
    {
      try {
        string jsonData = builder.ToString();
        //Console.WriteLine(jsonData);
        var data = JsonConvert.DeserializeObject<UpdataDataKwargs>(jsonData);
        if (data != null) {
          data.Version = -2;
          //Console.WriteLine("DATA: " + data.Data);
          //_dataService.UpdateDataAsync(data);
        }
      }
      catch {
        //Console.WriteLine("LUL: " + ex);
      }
    }

    public void Stop()
    {
      _cancelTokenSource.Cancel();
      //Log.Info("Stop client client: " + _url);
    }
  }
}