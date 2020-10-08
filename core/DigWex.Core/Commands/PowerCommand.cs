using System;
using System.Runtime.InteropServices;
using DigWex.Model.Commands;

namespace DigWex.Commands
{
  public class PowerCommand : Command
  {
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MONITORPOWER = 0xF170;
    private const int MONITOR_ON = -1;
    private const int MONITOR_OFF = 2;

    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern int PostMessage(IntPtr hWnd, int уMsg, IntPtr wParam, IntPtr lParam);

    private const int MOUSEEVENTF_MOVE = 0x0001;
    private readonly PowerKwargs _kwargs;

    private void Wake()
    {
      mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
      //Sleep(40);
      //mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero);
    }

    public PowerCommand(PowerCommandModel model) : base(model.Id)
    {
      _kwargs = model.Kwargs;
    }

    public override void StartAsync()
    {
      try {
        Log.Info($"Start set-power command, id: {CommandId}");
        if (_kwargs == null) {
          Log.Error($"Fail args set-power command, id: {CommandId}");
          return;
        }

        //Console.WriteLine("start command set-power");

        if (!_kwargs.On) {
          int state = PostMessage((IntPtr)0xFFFF, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)(MONITOR_OFF));
        }
        else {
          Wake();
        }

        //CommandService.RemovePriorityCommand(typeof(PowerCommandModel));

        Log.Info($"Complete power command, id: {CommandId}");
      }
      catch {
        Log.Error($"Error set-power command {CommandId}");
      }
    }
  }
}
