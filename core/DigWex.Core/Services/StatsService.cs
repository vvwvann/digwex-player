using DigWex.Api;
using DigWex.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace DigWex.Services
{
  class StatsService
  {
    private readonly static Lazy<StatsService> _instance =
      new Lazy<StatsService>(() => new StatsService());

    private readonly Timer _timer = new Timer();
    private readonly DeviceApi _deviceApi = DeviceApi.Instance;
    private readonly DataContext _context = DataContext.Instance;

    private St _last;

    public static StatsService Instance => _instance.Value;

    private StatsService()
    {
      _timer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
      _timer.Elapsed += async (e, v) => {
        _timer.Stop();
        try {
          await SendAsync();
        }
        catch { }
        _timer.Start();
      };
    }

    public void Start()
    {
      _timer.Start();

      Task.Run(async () => {
        await SendAsync();
      });
    }

    public void Add(int start, int id, int playlistId)
    {
      int end = Environment.TickCount;

      if (_last.Intervals == null) {
        _last.Unix = CorrectDateTime.UtcNow.UnixTime();
        _last.Last = start;
        _last.Intervals = new List<int>();
        _last.Contents = new List<int>();
      }

      int wait = Math.Abs(_last.Last - start) / 1000;
      int len = Math.Abs(end - start) / 1000;

      _last.Last = end;

      if (wait < 0) {
        _last.Intervals.Add(wait);
      }

      if (len < 1) return;


      _last.Intervals.Add(len);
      _last.Contents.Add(id);
      _last.Contents.Add(playlistId);

      //foreach (var item in _last.Intervals) {
      //  Console.Write(item + " ");
      //}
      //Console.WriteLine();

      Task.Run(async () => {
        try {
          var entity = new StatsEntity {
            Id = _last.Unix,
            Data = _last.ToJsonString()
          };
          await _context.InsertOrUpdateAsync(entity);
        }
        catch { }
      });


      if (_last.Intervals.Count > 720) {
        _last.Intervals = null;
      }
    }

    private async Task SendAsync()
    {
      List<StatsEntity> list = null;

      try {
        list = await _context.GetItems<StatsEntity>(100);
      }
      catch { }

      if (list == null || list.Count == 0) return;

      var dict = new Dictionary<int, KeyValuePair<int, int>>();
      var models = new List<St>();

      foreach (var item in list) {
        St model;
        try {
          model = item.Data.ToJsonObject<St>();
          models.Add(model);
        }
        catch { continue; }
        List<int> contents = new List<int>(model.Contents);
        if (contents == null || contents.Count % 2 == 1) continue;
        model.Contents.Clear();
        int k = 0;
        for (int i = 0; i < contents.Count; i += 2) {
          int id = contents[i];
          int playlistId = contents[i + 1];
          int index;
          if (dict.TryGetValue(id, out KeyValuePair<int, int> pair)) {
            index = pair.Key;
          }
          else {
            dict[id] = new KeyValuePair<int, int>(k, playlistId);
            index = k;
            k += 2;
          }
          model.Contents.Add(index);
        }
      }

      var obj = new {
        a = new List<int>(),
        s = models
      };

      var res = new Dictionary<int, object> {
        [1] = obj
      };

      foreach (var pair in dict) {
        obj.a.Add(pair.Key);
        obj.a.Add(pair.Value.Value);
      }

      bool ok = await _deviceApi.StatsAsync(res);

      if (ok) {
        foreach (var item in list) {
          try {
            await _context.DeleteItem(item);
          }
          catch { }
        }
      }
    }
  }
}

public struct St
{
  [JsonProperty("s")]
  public long Unix;
  [JsonIgnore]
  public int Last;
  [JsonProperty("c")]
  public List<int> Intervals;
  [JsonProperty("i")]
  public List<int> Contents;
}
