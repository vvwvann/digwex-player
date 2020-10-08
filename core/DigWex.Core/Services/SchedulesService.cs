using System;
using System.Timers;

namespace DigWex.Services
{
  public class SchedulesService
  {
    private readonly static Lazy<SchedulesService> _instance =
        new Lazy<SchedulesService>(() => new SchedulesService());

    private readonly Timer _timer;

    private object _locker = new object();
    //private volatile SettingsModel _model;

    public static SchedulesService Instance {
      get { return _instance.Value; }
    }

    public SchedulesService()
    {
      _timer = new Timer(60000);

      _timer.Elapsed += (e, v) => {
        DateTime dateTime = CorrectDateTime.Now;
        TryReboot(dateTime);
        SetBrightness(dateTime);
      };
      _timer.Start();
    }

    private void SetBrightness(DateTime dateTime)
    {
      //double value = 1.0;
      lock (_locker) {
        //if (_model?.BrightnessSchedule != null)
        //{
        //    foreach (BrightnessSchedule item in _model.BrightnessSchedule)
        //    {
        //        if (item.Time.TimeOfDay.TotalMinutes <= dateTime.TimeOfDay.TotalMinutes)
        //        {
        //            value = item.Value;
        //        }
        //    }
        //}
      }

      //App.Window.Dispatcher.Invoke(() => {
      //  App.Window.Opacity = value;
      //});
    }

    private void TryReboot(DateTime dateTime)
    {
      bool isReboot = false;
      lock (_locker) {
        //if (_model?.RebootSchedule != null)
        //{
        //    foreach (RebootSchedule item in _model.RebootSchedule)
        //    {
        //        if (item.Hour == dateTime.Hour && Math.Abs(dateTime.Minute - item.Minute) < 2)
        //        {
        //            isReboot = true;
        //            break;
        //        }
        //    }
        //}
      }

      if (isReboot) {
        Log.Info("Restart PC");
        try {
          System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }
        catch (Exception ex) {
          Log.Warn($"Shutdown proccess exception: {ex.Message}");
        }
      }
    }

    //public void SetSettings(SettingsModel model)
    //{
    //    lock (_locker)
    //        _model = model;
    //    DateTime dateTime = CorrectDateTime.Now;
    //    SetBrightness(dateTime);
    //}
  }
}
