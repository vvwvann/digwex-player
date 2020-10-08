using System;
using System.Threading.Tasks;
using DigWex.Network;

namespace DigWex.Manager
{
  public class SocketManager
  {
    private readonly static SocketManager _instance = new SocketManager();
    private BindWebSocket _longerSocket;
 
    public static SocketManager Instance => _instance;

    public async void StartLonger()
    {
      string scheme = "ws";
      Uri uri = Config.ServerUri;
      if (uri.Scheme == Uri.UriSchemeHttps)
        scheme += 's';

      string url = $"{scheme}://{uri.Host}:{uri.Port}/device/bind" +
        $"?authorization=OAuth {Uri.EscapeDataString(Config.ActivateData.Token)}";

      //Console.WriteLine(url);

      _longerSocket = new BindWebSocket(url);
      await _longerSocket.StartAsync();
    }
     

    public void StopLonger()
    {
      BindWebSocket socket = _longerSocket;
      socket?.StopAsync();
    }
    
  }
}