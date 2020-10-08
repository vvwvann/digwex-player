using System;
using System.Collections.Generic;
using DigWex.Api.Model;
using static DigWex.Api.Model.PackageModel;

namespace DigWex
{
  public abstract class MediaPlaylistBase
  {
    protected int _index = 0;
    protected int _packageId;
    public LinkedList<DefaultMedia> _medias = new LinkedList<DefaultMedia>();
    protected LinkedListNode<DefaultMedia> _nextNode;
    private ScheduleCondition _scheduleCondition;
    protected LinkedList<Uri> _audios = new LinkedList<Uri>();
    protected LinkedListNode<Uri> _nextAudio;

    public int Id { get; protected set; }

    public bool IsLastNext => _nextNode == _medias?.Last;

    public string Type { get; protected set; }

    public ScheduleCondition ScheduleCondition => _scheduleCondition;

    public MediaPlaylistBase(PlaylistModel playlist, List<ScheduleModel> schedules, int packageId)
    {
      Id = playlist.Id;
      _packageId = packageId;

      if (schedules != null && schedules.Count != 0)
        _scheduleCondition = new ScheduleCondition(schedules, playlist);
    }

    public virtual DefaultMedia NextMedia()
    {
      DefaultMedia media = _nextNode?.Value;
      _nextNode = _nextNode?.Next ?? _medias.First;
      return media;
    }

    public virtual Uri NextAudio()
    {
      throw new NotImplementedException();
    }

    public virtual void ResetIndex()
    {
      _nextNode = _medias.First;
    }


    public abstract bool Create(PlaylistModel items, PackageModel model);

    public virtual DefaultMedia NextMedia(int id)
    {
      throw new NotImplementedException();
    }

    public virtual void Dispose()
    {

    }

    public DefaultMedia CurrentMedia()
    {
      return _nextNode?.Value;
    }

    public void SetPositionToMedia(DefaultMedia media)
    {
      if (media == null) return;

      LinkedListNode<DefaultMedia> start = _medias.First;

      while (start != null) {
        DefaultMedia val = start.Value;
        if (val.Main.Url == media.Main.Url) {
          _nextNode = start;
          return;
        }
        start = start.Next;
      }
    }
  }
}