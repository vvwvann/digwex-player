using DigWex.Api.Model;
using DigWex.Commands;
using DigWex.Model;
using System.Collections.Concurrent;

namespace DigWex.Managers
{
  public class CommandManager
  {
    private const int SYNC = 1;
    private const int REBOOT = 2;
    private const int LOG = 4;
    private const int SCREENSHOT = 8;
    private const int SPLASH = 16;
    private const int SETTINGS = 32;

    private static readonly CommandManager _instance = new CommandManager();

    private readonly ConcurrentDictionary<int, int> _commandPriority = new ConcurrentDictionary<int, int>(1, 32);

    public static CommandManager Instance => _instance;

    public void RemovePriorityCommand(int type)
    {
      _commandPriority.TryRemove(type, out int cnt);
    }

    public bool CheckCommand(int bits, int mask)
    {
      int bit = bits & mask;
      return bit == mask;
      //_commandPriority.TryGetValue(bit, out int cnt);
      //return cnt == 0;
    }

    public void StartCommands(ComunicateServerModel response)
    {
      int bits = response.commands;
      ICommand command = null;

      if (CheckCommand(bits, SYNC)) {
        command = new SynchronizeCommand();
        //_commandPriority[SYNC] = 1;
      }
      if (CheckCommand(bits, REBOOT)) {
        command = new RebootCommand();
      }
      if (CheckCommand(bits, LOG)) {
        command = new UploadLogCommand();
        //_commandPriority[LOG] = 1;
      }
      if (CheckCommand(bits, SCREENSHOT)) {
        command = new TakeScreenshotCommand();
        //_commandPriority[SCREENSHOT] = 1;
      }
      if (CheckCommand(bits, SPLASH)) {
        command = new SplashCommand();
        //_commandPriority[SCREENSHOT] = 1;
      }
      if (CheckCommand(bits, SETTINGS)) {
        if (response.extra != null)
          command = new SettingsCommand(response.extra[SETTINGS]);
        //_commandPriority[SCREENSHOT] = 1;
      } 
      command?.StartAsync();
    }

    public void StartSyncCommand()
    {
      StartCommands(new ComunicateServerModel() { commands = 1 });
    }
  }
}
