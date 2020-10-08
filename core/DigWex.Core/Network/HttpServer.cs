using DigWex.Extensions;
using DigWex.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigWex.Network
{
  public class HttpServer
  {
    public event EventHandler<EventArgs> MediaEnded = (e, v) => { };
    public event EventHandler<EventArgs> PreloadMedia = (e, v) => { };
    public event EventHandler<EventArgs> OnConfig = (e, v) => { };

    private WebSocket _socket;
    private HttpListener _httpListener = new HttpListener();

    private static readonly Lazy<HttpServer> _instance =
        new Lazy<HttpServer>(() => new HttpServer());

    public static HttpServer Instance => _instance.Value;
    private bool _config;
 
    public async void Connect(int port)
    {
      try {
        if (!_httpListener.IsListening) {
          _httpListener.Prefixes.Add($"http://localhost:{port}/");
          _httpListener.Start();
        }
        Console.WriteLine("HTTP CONNECT");
        while (true) {
          await LoopReceiveAsync();
          Program.ExitApp(0);
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
    }

    private async Task LoopReceiveAsync()
    {
      try {
        HttpListenerContext context = await _httpListener.GetContextAsync();
        if (context.Request.IsWebSocketRequest) {
          HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
          _socket = webSocketContext.WebSocket;
          while (_socket.State == WebSocketState.Open) {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            string message = null;
            WebSocketReceiveResult result = null;
            StreamReader reader = null;
            var ms = new MemoryStream();
            try {
              do {
                result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
              }
              while (!result.EndOfMessage);

              ms.Seek(0, SeekOrigin.Begin);
              reader = new StreamReader(ms, Encoding.UTF8);
              message = await reader.ReadToEndAsync();
            }

            catch (Exception ex) { Console.WriteLine(ex); }
            finally {
              reader?.Dispose();
            }
            if (result?.MessageType == WebSocketMessageType.Text) {
              DeserializeMessage(message);
              if (!_config)
                Console.WriteLine("WAIT CONFIG");
              await Task.Delay(1000);

            }
          }
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
    }

    private void DeserializeMessage(string message)
    {
      switch (message) {
        case "preload": PreloadMedia(null, null); return;
        case "end": MediaEnded(null, null); return;
      }
      try {
        if (_config) {
          MediaEnded(null, null); return;
        };

        ConfigWebsocketModel config = message.ToJsonObject<ConfigWebsocketModel>();
        if (config != null) {
          bool ok = Config.Instance.Init(config);
          if (ok) {
            _config = true;
            OnConfig(null, null);
          }
        }
      }
      catch (Exception ex) { Console.WriteLine(ex); }
    }

    public async Task SendCommandAsync(string command)
    {
      try {
        await _socket.SendAsync(new ArraySegment<byte>(command.ToByte()), WebSocketMessageType.Text, true, CancellationToken.None);
      }
      catch { }
    }

    public async Task SendCommandAsync(object command)
    {
      string data = command.ToJsonString();
      try {
        await _socket.SendAsync(new ArraySegment<byte>(data.ToByte()), WebSocketMessageType.Text, true, CancellationToken.None);
      }
      catch { }
    }

    public async Task SendCommandAsync(DefaultMedia media)
    {
      try {
        string command = new { media, next = media.NextMedia }.ToJsonString();
        await _socket.SendAsync(new ArraySegment<byte>(command.ToByte()), WebSocketMessageType.Text, true, CancellationToken.None);
      }
      catch (Exception ex) { Console.WriteLine(ex); }
    }
  }
}
