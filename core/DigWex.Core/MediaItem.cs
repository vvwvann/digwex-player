using Newtonsoft.Json;
using System;

namespace DigWex
{
  public class MediaItem
  {
    private readonly DataInfo _data;
    private readonly int _id;
    private readonly Uri _url;
    private readonly int[] _position;
    private readonly string _type;

    [JsonProperty("id")]
    public int Id => _id;

    [JsonIgnore]
    public DataInfo Data => _data;

    [JsonProperty("url")]
    public Uri Url => _url;

    [JsonProperty("position")]
    public int[] Position => _position;

    [JsonProperty("type")]
    public string Type => _type;


    public MediaItem(Uri uri, string type, int[] position, DataInfo data, int id)
    {
      _id = id;
      _url = uri;
      _type = type;
      _position = position;
      _data = data;
    }
  }
}
