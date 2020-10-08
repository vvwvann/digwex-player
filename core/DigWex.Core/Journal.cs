using System.Collections.Generic;
using DigWex.Model;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace DigWex
{
  public class Journal
  {
    private static readonly Lazy<Journal> _instance =
        new Lazy<Journal>(() => new Journal());

    public SynchronizationModel _syncInfo = null;

    public static bool Power { get; set; } = true;

    public event EventHandler<int> SynchronizeProgress = (e, v) => { };

    public event EventHandler SynchronizeComplete = (s, e) => { };

    public static Journal Instance {
      get {
        return _instance.Value;
      }
    }

    public void SetSyncInfo(SynchronizationModel model, int percent)
    {
      SynchronizeProgress(this, percent);
      if (model == null)
        SynchronizeComplete(this, null);
      _syncInfo = model;
    }

    public SynchronizationModel GetSyncInfo()
    {
      return _syncInfo;
    }
  }
}