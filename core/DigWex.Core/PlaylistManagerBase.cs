using System;
using System.Collections.Generic;
using DigWex.Managers;
using DigWex.Services;

namespace DigWex
{
  public abstract class PlaylistManagerBase<T> : IDisposable where T : IPackageManager
  {
    private readonly object _locker = new object();
    protected readonly DataContext _dataContext;
    protected readonly T _packageService;

    protected Dictionary<int, MediaPlaylistBase> _schedulePlaylists;
    protected MediaPlaylistBase _currPlaylist;
    protected int _packageId = -1;

    public PlaylistManagerBase()
    {
      _dataContext = DataContext.Instance;
      _schedulePlaylists = new Dictionary<int, MediaPlaylistBase>();
      _packageService = (T)PackageManager.Instance;

      _packageService.UpdatedPlaylist += _packageService_UpdatedPlaylist;

      UpdatePlaylist(_packageService.PackageId);
    }

    private void _packageService_UpdatedPlaylist(object sender, bool force)
    {
      Dictionary<int, MediaPlaylistBase> dict = _packageService.MediaPlaylists;
      lock (_locker) {
        foreach (var playlist in _schedulePlaylists.Values)
          playlist.Dispose();

        _schedulePlaylists.Clear();

        foreach (var item in dict)
          _schedulePlaylists.Add(item.Key, item.Value);

        DefaultMedia current = _currPlaylist?.CurrentMedia();
        SetMediaPlaylist();
        _currPlaylist?.SetPositionToMedia(current);
        if (force)
          ForceSync();
      }
    }

    public int PackageId => _packageId;

    public abstract void ForceSync();

    public DefaultMedia GetNext()
    {
      lock (_locker) {
        SetMediaPlaylist();

        //if (_currPlaylist != null)
        //{
        //    _currPlaylist.TriggerData.Rrule.IsEnableNextTime = _currPlaylist.IsLastNext;
        //}

        return _currPlaylist?.NextMedia();
      }
    }

    public virtual void JournalUpdate(DefaultMedia media, DateTime startTime)
    {
      //JournalItem item = new JournalItem(JournalTypes.PlaybackItemPlayback)
      //{
      //  Data = new PlaybackItemPlaybackModel
      //  {
      //    DeviceDataId = media.PackageId,
      //    PlaybackItemId = media.PlaybackItemId,
      //    TriggerId = media.TriggerId,
      //    Start = startTime,
      //    End = CorrectDateTime.UtcNow
      //  }
      //};
      //Journal.Instance.AddItem(item);
    }

    public void UpdateRule(DefaultMedia media, DateTime startTime)
    {
      //try
      //{
      //    if (media != null)
      //    {
      //        if (media.IsFirst)
      //        {
      //            if (_currPlaylist.TriggerData.Rrule.Count != null)
      //            {
      //                _currPlaylist.TriggerData.Rrule.Increment++;
      //                await _dataContext.UpdateItem(new Info
      //                {
      //                    Id = _currPlaylist.Id,
      //                    Count = _currPlaylist.TriggerData.Rrule.Increment
      //                });
      //            }
      //            if (_currPlaylist.TriggerData.Rrule.Interval != null)
      //            {
      //                DateTime time = CorrectDateTime.GetLocalTime(startTime);
      //                _currPlaylist.TriggerData.Rrule.NextTime = time.AddSeconds((double)_currPlaylist.TriggerData.Rrule.Interval);
      //            }
      //        }
      //    }
      //}
      //catch { }
    }

    private void SetMediaPlaylist()
    {
      DateTime curr = CorrectDateTime.Now;
      foreach (MediaPlaylistBase item in _schedulePlaylists.Values) {
        bool ok = item.ScheduleCondition?.Exist(curr) ?? true;
        Console.WriteLine("OK: " + ok);

        if (ok) {
          _currPlaylist = item;
          return;
        }
      }
      _currPlaylist = null;
    }

    public void UpdatePlaylist(int id)
    {
      lock (_locker) {
        if (_packageId != id && id != -1) {
          _packageId = id;
          Dictionary<int, MediaPlaylistBase> dict = _packageService.MediaPlaylists;

          foreach (var playlist in _schedulePlaylists.Values)
            playlist.Dispose();

          _schedulePlaylists.Clear();

          foreach (var item in dict)
            _schedulePlaylists.Add(item.Key, item.Value);
          SetMediaPlaylist();
        }
      }
    }

    public virtual void Play()
    {

    }

    public void Dispose()
    {
      foreach (var item in _schedulePlaylists.Values) {
        item.Dispose();
      }
    }
  }
}