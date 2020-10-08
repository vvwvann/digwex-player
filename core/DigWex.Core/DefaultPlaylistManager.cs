using System;
using System.Collections.Generic;
using System.Timers;
using DigWex.Managers;
using DigWex.Network;

namespace DigWex
{
  public class DefaultPlaylistManager : PlaylistManagerBase<DefaultPackageManager>
  {
    private Timer _timer;
    private readonly Stack<MediaPlaylistBase> _stackExternalPlaylist = new Stack<MediaPlaylistBase>();
    private HttpServer _http = HttpServer.Instance;
    private DefaultMedia _media;

    public DefaultPlaylistManager(/*DefaultPlayer player*/)
    {
      //Player = player;
      _timer = new Timer(1000);
      _timer.Elapsed += _timer_Elapsed;
      _packageService.PushedExternalPlaylist += PushedExternalPlaylist;
      _http.PreloadMedia += PreloadMedia;
      _http.MediaEnded += MediaEnded;
      
    }

    private void MediaEnded(object sender, EventArgs e)
    {
      Play();
    }

    public override void ForceSync()
    {
      Start();
    }

    private void PushedExternalPlaylist(object sender, MediaPlaylistBase playlist)
    {
      if (playlist == null) return;
      SetExternalPlaylist(playlist);
    }

    public Uri NextAudio()
    {
      return _currPlaylist?.NextAudio();
    }

    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _timer.Stop();
      Play();
    }

    private void SetExternalPlaylist(MediaPlaylistBase playlist)
    {
      if (_currPlaylist?.Id != playlist.Id) {
        _stackExternalPlaylist.Push(_currPlaylist);
        _currPlaylist = playlist;
      }
      //Player.ForceEnd();
      _currPlaylist.ResetIndex();
    }

    public override async void Play()
    {
      Console.WriteLine("PLAY");
      if (_stackExternalPlaylist.Count > 0) {
        _media = _currPlaylist?.NextMedia();
        if (_media == null) {
          _currPlaylist = _stackExternalPlaylist.Pop();
          _media = _currPlaylist?.NextMedia();
        }
      }
      else {
        _media = GetNext();
      }

      if (_media == null) {
        _timer.Start();
        await _http.SendCommandAsync(new { wait = true });
      }
      else {
        Console.WriteLine($"Play {_media.Main.Type}, {_media.Main.Url}, duration: {_media.Duration.TotalSeconds}");
        await _http.SendCommandAsync(_media);
      }
      //Player.Play(media);
    }

    private async void PreloadMedia(object sender, EventArgs e)
    {
      await _http.SendCommandAsync(_media.NextMedia);
    }

    public virtual void Start()
    {
      _timer.Start();
    }

    public virtual void Stop()
    {
      _timer.Stop();
      //Dispose();
    }
  }
}
