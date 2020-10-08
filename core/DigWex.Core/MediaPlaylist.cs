using System;
using System.Collections.Generic;
using DigWex.Api.Model;
using DigWex.Managers;
using static DigWex.Api.Model.PackageModel;

namespace DigWex
{
  public class MediaPlaylist : MediaPlaylistBase
  {
    private readonly object _locker = new object();
    private readonly object _lockerAudio = new object();

    private LinkedList<DefaultMedia> _tmpMedias = new LinkedList<DefaultMedia>();
    private DefaultPackageManager _packageManager;

    public MediaPlaylist(PlaylistModel playlistModel, List<ScheduleModel> schList, int packageId) :
        base(playlistModel, schList, packageId)
    {
      _packageManager = (DefaultPackageManager)PackageManager.Instance;
    }

    public override bool Create(PlaylistModel playlist, PackageModel model)
    {
      var medias = new List<DefaultMedia>();
      var audios = new LinkedList<Uri>();
      foreach (var item in playlist.Main) {
        DefaultMedia media = new DefaultMedia(_packageId, playlist.Id, -1);
        bool ok = media.Create(item, model, playlist.Widgets);
        if (ok) {
          medias.Add(media);
        }
      }
      if (medias.Count == 0) return false;

      if (playlist.Audios != null && playlist.Audios.Length != 0) {
        foreach (var ind in playlist.Audios) {
          audios.AddLast(model.Contents[ind].Path);
        }
        _audios = audios;
      }

      int n = medias.Count;
      medias[0].IsFirst = true;
      medias[n - 1].IsLast = true;

      for (int i = 0; i < n; i++) {
        DefaultMedia media = medias[i];
        DefaultMedia nextMedia = medias[(i + 1) % medias.Count];
        media.NextMedia = nextMedia;
        _medias.AddLast(media);
        _tmpMedias.AddLast(media);
      }

      _nextAudio = _audios.First;
      _nextNode = _medias.First;
      return true;
    }

    public override Uri NextAudio()
    {
      lock (_lockerAudio) {
        Uri audio = _nextAudio?.Value;
        _nextAudio = _nextAudio?.Next ?? _audios.First;
        Console.WriteLine("NEXT AUDIO IS" + audio);
        return audio;
      }
    }


    public override DefaultMedia NextMedia()
    {
      lock (_locker) {
        // if playlist modify
        //if (_nextNode == _medias.First) {
        //  ResetPlaylist();
        //}

        DefaultMedia media = _nextNode.Value;

#if ADVELIT
        while (!media.IsPlay()) {
          _nextNode = _nextNode?.Next ?? _medias.First;
          media = _nextNode?.Value;
        }
#endif
        _nextNode = _nextNode?.Next ?? _medias.First;
        return media;
      }
    }

    private void ResetPlaylist()
    {
      _medias = new LinkedList<DefaultMedia>(_tmpMedias);

      LinkedListNode<DefaultMedia> next = _medias.First;
      DefaultMedia prev = _medias.Last.Value;
      while (next != null) {
        DefaultMedia media = next.Value;
        prev.NextMedia = media;
        prev = media;
        next = next.Next;
      }

      _medias.First.Value.IsFirst = true;
      _medias.Last.Value.IsLast = true;
      _nextNode = _medias.First; _medias.Clear();

      foreach (var item in _tmpMedias)
        _medias.AddLast(item);

      _nextNode = _medias.First;

      Console.WriteLine("Clear medias");
    }
  }
}