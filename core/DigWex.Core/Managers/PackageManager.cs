using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigWex.Managers
{
  public interface IPackageManager
  {
    int PackageId { get; }

    Dictionary<int, MediaPlaylistBase> MediaPlaylists { get; }

    Task InitAsync();

    event EventHandler<bool> UpdatedPlaylist;
  }

  public sealed class PackageManager
  {
    private static IPackageManager _instance;

    public static IPackageManager Instance => _instance;

    public static void Init()
    {
      _instance = new DefaultPackageManager();
    }
  }
}