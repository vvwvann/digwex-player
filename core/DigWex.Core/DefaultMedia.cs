using System;
using System.Windows;
using System.Collections.Generic;
using static DigWex.Api.Model.PackageModel;
using DigWex.Api.Model;
using DigWex.Model;
using DigWex.Managers;
using DigWex.Services;
using Newtonsoft.Json;

namespace DigWex
{
  public class DefaultMedia : ICloneable
  {
    #region Fields

    protected readonly int _packageId;

    protected readonly int _playlistId;

    protected int? _playAudio;

    protected int _uniqId;

    protected readonly int _playbackItemId;

    protected TimeSpan _duration;

    protected LinkedList<MediaItem> _widgets = new LinkedList<MediaItem>();

    protected MediaItem _main;

    protected double _totalSecond;

    protected DefaultMedia _nextMedia;

    protected int _filter;

    #endregion

    [JsonIgnore]
    public int PackageId => _packageId;

    [JsonIgnore]
    public int PlaylistId => _playlistId;

    [JsonIgnore]
    public int PlaybackItemId => _playbackItemId;

    [JsonIgnore]
    public TimeSpan Duration => _duration;

    [JsonProperty("widgets")]
    public LinkedList<MediaItem> Widgets => _widgets;

    [JsonProperty("totalSecond")]
    public double TotalSecond => _totalSecond;

    [JsonProperty("main")]
    public MediaItem Main => _main;

    public bool IsFirst { get; set; }

    [JsonIgnore]
    public bool IsLast { get; set; }

    [JsonIgnore]
    public DefaultMedia NextMedia { get; set; }

    [JsonIgnore]
    public int Filter => _filter;

    public int? PlayAudio => _playAudio;

    [JsonIgnore]
    public bool[] DaysOfWeek { get; set; } = new bool[7];

    [JsonIgnore]
    public int From { get; set; }

    [JsonIgnore]
    public int To { get; set; }

    private readonly DefaultPackageManager _packageManager
      = (DefaultPackageManager)PackageManager.Instance;

    public DefaultMedia(int packageId, int playlistId, int playbackItemId)
    {
      _packageId = packageId;
      _playlistId = playlistId;
      _playbackItemId = playbackItemId;
    }

    private ItemModel GetProperties(long item, PackageModel model)
    {
      ItemModel it = new ItemModel();
      uint mask16 = 0xFFFF, mask32 = 0xffffffff;

      int indContent = (int)(item & mask16);
      it.Content = model.Contents[indContent];
      item >>= 16;
      int indPos = (int)(item & mask16);
      if (indPos > 0) {
        indPos--;
        it.Position = new int[4]{ model.Positions[indPos],
          model.Positions[indPos + 1],
          model.Positions[indPos + 2],
          model.Positions[indPos + 3],
          };
      }

      item >>= 16;
      int dataOrTemplate = (int)(item & mask32);

      if (dataOrTemplate < 0) {
        DataModel value = _packageManager.GetValuesData(-dataOrTemplate);
        if (value != null) {
          it.Data = value;
        }
      }
      else if (dataOrTemplate > 0) {
        it.Template = model.Templates[--dataOrTemplate];
      }

      return it;
    }

    public virtual bool Create(MainModel media, PackageModel model, long[] widgets)
    {
      ItemModel item = GetProperties(media.Content, model);
      Uri uri = item.Content.Path;
      if (media.Duration < 2
        || uri == null
        && !Uri.TryCreate(item.Content.Url, UriKind.Absolute, out uri)) {
        return false;
      }

      var data = DataInfo.TryCreate(item.Data, item.Template);
      _main = new MediaItem(uri, item.Content.Type, item.Position, data, item.Content.Id);
      _duration = TimeSpan.FromSeconds(media.Duration);
      _totalSecond = media.Duration;
      Console.WriteLine("PLAY AUDIO:" + _playAudio);
      _playAudio = media.PlayAudio;
      _filter = media.Filter;
      DaysOfWeek = media.DaysOfWeek;
      From = media.From;
      To = media.To;

      if (widgets != null) {
        for (int i = 0; i < widgets.Length; i++) {
          item = GetProperties(widgets[i], model);
          uri = item.Content.Path;
          if (uri == null && !Uri.TryCreate(item.Content.Url, UriKind.Absolute, out uri)) return false;
          data = DataInfo.TryCreate(item.Data, item.Template);
          var tmp = new MediaItem(uri, item.Content.Type, item.Position, data, -1);
          _widgets.AddLast(tmp);
        }
      }
      return _main != null;
    }

    public bool IsPlay()
    {
      var day = CorrectDateTime.Now;
      Console.WriteLine("Day: " + day);
      if (!DaysOfWeek[(int)day.DayOfWeek]) return false;

      int minutes = (int)day.TimeOfDay.TotalMinutes;
      Console.WriteLine($"MEDIA PLAY FROM {From} TO {To}");

      return minutes >= From && minutes <= To;
    }

    public object Clone()
    {
      return MemberwiseClone();
    }
  }
}
