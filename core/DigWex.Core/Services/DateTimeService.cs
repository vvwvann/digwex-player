using System;
using System.Threading.Tasks;
using System.Timers;

namespace DigWex.Services
{
  public class DateTimeService
  {
    private readonly static Lazy<DateTimeService> _instance =
      new Lazy<DateTimeService>(() => new DateTimeService());

    private readonly Timer _timer;

    public static double Offset { get; private set; } = DateTimeOffset.Now.Offset.TotalSeconds;
    public static double Delta { get; private set; }

    public static DateTimeService Instance => _instance.Value;


    DateTimeService()
    {
      Console.WriteLine("Offset compare local: " + Offset);

      _timer = new Timer(120000) {
        Enabled = false,
        AutoReset = true
      };
      _timer.Elapsed += async (e, v) => {
        _timer.Stop();
        await SetServerTime();
        _timer.Start();
      };
    }

    public async void Start()
    {
      _timer.Start();
      await SetServerTime();
    }

    public void Stop()
    {
      _timer.Stop();
    }

    private async Task SetServerTime()
    { 
      var dateNullable = await Api.DeviceApi.Instance.GetServerTime();
      if (dateNullable != null) {
        DateTime date = dateNullable.Utc;
        try {
          double offset = dateNullable.Offset * 60;
          DateTime currUtc = DateTime.UtcNow;
          DateTime localTime = date.AddSeconds(offset);
          double delta = date.Subtract(currUtc).TotalSeconds;
          double totalSeconds = offset + delta;

          Console.WriteLine("Offset compare server: " + offset);
          Console.WriteLine("Server dateTime: " + date);
          Console.WriteLine("Local dateTime: " + localTime);

          Offset = offset;
          Delta = delta;
        }
        catch { }
      }
    }
  }

  public static class CorrectDateTime
  {
    public static DateTime UtcNow {
      get {
        return DateTime.UtcNow.AddSeconds(DateTimeService.Delta);
      }
    }

    public static DateTime Now {
      get {
        double totalSeconds = DateTimeService.Delta + DateTimeService.Offset;
        return DateTime.UtcNow.AddSeconds(totalSeconds);
      }
    }
  }
}
