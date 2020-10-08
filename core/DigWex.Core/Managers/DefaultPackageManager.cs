using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigWex.Services;
using Newtonsoft.Json.Linq;
using DigWex.Utils;
using System.Collections.Concurrent;
using DigWex.Api.Model;
using static DigWex.Api.Model.PackageModel;

namespace DigWex.Managers
{
  public class DefaultPackageManager : PackageManagerBase
  {
    private readonly ConcurrentDictionary<int, DataModel> _datas
      = new ConcurrentDictionary<int, DataModel>();
    private volatile Dictionary<string, int> _externalMap
      = new Dictionary<string, int>();

    private readonly ExternalService _externalService;
    private readonly DataService _dataService;

    public ScheduleModel[] _schedules;
    public ContentModel[] _contents;

    public DefaultPackageManager()
    {
      _externalService = ExternalService.Instance;
      _dataService = DataService.Instance;
    }

    public void OnPushExternalPlaylist(string key)
    {
      bool ok = _externalMap.TryGetValue(key, out int id);
      if (ok) {
        //_scHplaylists.TryGetValue(id, out IMediaPlaylist val);
        //PushedExternalPlaylist(null, val);
      }
    }

    public DataModel GetValuesData(int id)
    {
      _datas.TryGetValue(id, out DataModel val);
      return val;
    }

    public override async Task UpdateBase(DataModel[] data)
    {
      var used = new HashSet<string>();
      var noRemoves = new HashSet<int>();

      foreach (var item in data) {
        DataModel lastData = item;

        // if (item.Url != null) {
        //used.Add(item.Url);

        DataEntity clientData = await _dataContext.GetClientData(item.Id);

        if (clientData != null) {
          JToken obj = JsonUtils.TryParse(clientData.Data);

          lastData = new DataModel {
            Id = item.Id,
            Data = obj,
            Version = -2
          };

          // }
        }
        noRemoves.Add(item.Id);
        if (lastData != null)
          _datas[item.Id] = lastData;
      }

      List<DataEntity> items = await _dataContext.GetItems<DataEntity>(1000);

      var removes = new List<DataEntity>();

      foreach (DataEntity item in items) {
        if (!noRemoves.Contains(item.Id))
          removes.Add(item);
      }

      await _dataContext.DeleteItems(removes);

      foreach (var item in _datas)
        if (!noRemoves.Contains(item.Key))
          _datas.TryRemove(item.Key, out var val);

      _dataService.Update(used);
    }

    public async void UpdateDataAsync(DataModel data)
    {
      if (data == null) return;
      if (_datas.TryGetValue(data.Id, out DataModel val)) {
        if (val.Version > data.Version && data.Version != -2) return;
      }

      val.Version = data.Version;
      val.Data.Replace(data.Data);

      //_datas[data.Id] = data;

      string json = data.Data?.ToString(Newtonsoft.Json.Formatting.None) ?? "";

      //Console.WriteLine(json);

      await _dataContext.InsertOrUpdateAsync(new DataEntity {
        Id = data.Id,
        Data = json
      });

      DataChanged(this, data.Id);
    }

    public override async Task UpdatePackageAsync(PackageModel model)
    {
      if (model == null) return;
      Console.WriteLine("UPDATE");
      var dict = new Dictionary<int, MediaPlaylistBase>();
      var datas = model.Data ?? new DataModel[0];
      var playlists = model.Playlists ?? new PlaylistModel[0];

      _contents = model.Contents ?? new ContentModel[0];
      _schedules = model.Schedules ?? new ScheduleModel[0];


      foreach (var pl in playlists) {
        List<ScheduleModel> list = new List<ScheduleModel>();
        if (pl.Schedules != null) {
          foreach (int ind in pl.Schedules) {
            list.Add(_schedules[ind]);
          }
        }
        MediaPlaylistBase playlist = new MediaPlaylist(pl, list, model.Id);
        if (playlist != null) {
          bool ok = playlist.Create(pl, model);
          if (ok) {
            dict[pl.Id] = playlist;
          }
        }
      }
      _playlists = dict;
      _packageId = model.Id;
      await Task.Delay(10);
      OnUpdatePlaylist(model.Force);
    }

    public event EventHandler<int> DataChanged = (e, v) => { };

    public event EventHandler<MediaPlaylistBase> PushedExternalPlaylist = (e, v) => { };
  }
}