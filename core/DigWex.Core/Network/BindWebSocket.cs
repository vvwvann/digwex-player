using System;
using DigWex.Model;
using DigWex.Extensions;
using System.Threading.Tasks;
using DigWex.Managers;
using DigWex.Utils;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;

namespace DigWex.Network
{
  public class BindWebSocket
  {
    private const string REASON_NO_AUTH = "NO_OAUTH";
    private readonly CommandManager _commandService;
    private readonly LongerRequest _request;
    private ClientWebSocket _clientWebSocket;
    private string _url;

    public BindWebSocket(string url)
    {
      _clientWebSocket = new ClientWebSocket();
      _url = url;

      _commandService = CommandManager.Instance;
      _request = new LongerRequest();
    }


    public async Task StartAsync()
    {
      try {
        if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open) {
          _clientWebSocket?.Dispose();
          _clientWebSocket = new ClientWebSocket();
        }
        else return;

        while (_clientWebSocket.State != WebSocketState.Open)
          await _clientWebSocket.ConnectAsync(new Uri(_url), CancellationToken.None);

        await SendAsync();
        await LoopReceiveAsync();
      }
      catch {
        await StartAsync();
      }
    }

    private async Task LoopReceiveAsync()
    {
      while (_clientWebSocket.State == WebSocketState.Open) {
        try {
          ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
          string message = null;
          WebSocketReceiveResult result = null;
          StreamReader reader = null;
          MemoryStream ms = new MemoryStream();

          try {
            do {
              result = await _clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
              ms.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            reader = new StreamReader(ms, Encoding.UTF8);
            message = await reader.ReadToEndAsync();
            //Console.WriteLine(serializedMessage);
          }
          catch (Exception ex) { Console.WriteLine(ex); }
          finally {
            reader?.Dispose();
          }

          if (result?.MessageType == WebSocketMessageType.Text) {
            var model = message.ToJsonObject<ComunicateServerModel>();
            if (model != null && model.commands > 0) {
              _commandService.StartCommands(model);
            }
          }
        }
        catch (Exception ex) {
          Console.WriteLine(ex);
        }
      }

      if (await App.UnpairAsync()) return;
      await Task.Delay(5000);
      await StartAsync();
    }

    private async Task SendAsync()
    {
      try {
        ComunicateClientModel model = _request.CreateRequest();

        await _clientWebSocket.SendAsync(new ArraySegment<byte>(model.ToJsonByte()), WebSocketMessageType.Text, true, CancellationToken.None);

        Console.WriteLine("Send: {0}", model.ToJsonString());
      }
      catch (Exception ex) {
        Console.WriteLine("Send exception: {0}", ex.Message);
      }
    }

    public async Task StopAsync()
    {
      try {
        if (_clientWebSocket.State != WebSocketState.Closed)
          await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
      }
      catch { }
    }
  }

  public class LongerRequest
  {
    private const string OS_KEY = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";

    private readonly Journal _journal;
    private readonly CommandManager _commandManager;

    private readonly ComunicateClientModel.Telemetry _telemety;
    private DateTime _lastSendTelemetry;

    public int? _noArgsCommands;

    public LongerRequest()
    {
      _telemety = new ComunicateClientModel.Telemetry {
        version = Program.Version
      };
      _journal = Journal.Instance;
      _commandManager = CommandManager.Instance;
      _lastSendTelemetry = DateTime.MinValue;
    }

    public ComunicateClientModel CreateRequest()
    {
      //var time = DateTime.UtcNow;
      var shema = new ComunicateClientModel {
        //CommandsAcknowledge = _commandManager.GetAcknowledge(),
        display = Journal.Power,
        telemetry = _telemety
        //Ulogs = await _journal.GetNext(),
        //Synchronization = _journal.GetSyncInfo()
      };

      UpdateTelemetry();

      //int sub = time.Subtract(_lastSendTelemetry).Minutes;
      //if (sub >= 5)
      //{
      //  //UpdateTelemetry();
      //  _lastSendTelemetry = time;
      //  _telemety.Time = time;
      //  shema.telemetry = _telemety;
      //}
      return shema;
    }

    private void UpdateTelemetry()
    {
      //SetInfoDrive(out AvaliableTotal root, Config.RootDrive);
      //SetInfoDrive(out AvaliableTotal data, Config.ContentDrive);

      //_telemety.drive = new long[] { data.Available, data.Total };

      if (_telemety.osVersion == null)
        _telemety.osVersion = GetOSName();

      _telemety.networkMac = NetworkUtils.GetMacAddress()?.ToString();
      _telemety.networkIp = NetworkUtils.LocalAddress();
      ScreenBound screen = Config.ScreenBound;

      if (screen != null) {
        _telemety.resolution = new int[] {
          screen.width,
          screen.height
        };
      }
      //_telemety.resolution = new int[] {
      //          Screen.PrimaryScreen.Bounds.Width,
      //          Screen.PrimaryScreen.Bounds.Height
      //      };
    }

    private void SetInfoDrive(out AvaliableTotal val, string driveName)
    {
      var info = DriveUtils.GetInfoByDisk(driveName);
      if (info != null) {
        try {
          val = new AvaliableTotal {
            Available = info.AvailableFreeSpace,
            Total = info.TotalSize
          };
          return;
        }
        catch { }
      }
      val = null;
    }

    private string GetOSName()
    {
      ////RegistryKey localMachine = Registry.LocalMachine;
      //try
      //{
      //  RegistryKey skey = localMachine.OpenSubKey(OS_KEY);
      //  return (string)skey.GetValue("ProductName");
      //}
      //catch { }
      //return null;
      return "";
    }
  }
}