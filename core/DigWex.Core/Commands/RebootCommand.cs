using System;

namespace DigWex.Commands
{
  public class RebootCommand : Command
  {
    public override void StartAsync()
    {
      long tick = Environment.TickCount;
      if (tick == Config.LastRebootId) {
        Log.Info($"Complete reboot command, tick: {tick}");
      }
      else {
        Config.LastRebootId = tick;
        Log.Info($"Start reboot command, tick: {tick}");
        Program.ExitApp(-1);
      }
    }
  }
}
