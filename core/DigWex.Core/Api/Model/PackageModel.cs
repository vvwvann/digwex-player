using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DigWex.Api.Model
{
  public class PackageModel
  {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("force")]
    public bool Force { get; set; }

    [JsonProperty("schedules")]
    public ScheduleModel[] Schedules { get; set; }

    [JsonProperty("playlists")]
    public PlaylistModel[] Playlists { get; set; }

    [JsonProperty("positions")]
    public int[] Positions { get; set; }

    [JsonProperty("templates")]
    public JToken[] Templates { get; set; }

    [JsonProperty("contents")]
    public ContentModel[] Contents { get; set; }

    [JsonProperty("data")]
    public DataModel[] Data { get; set; }



    public class DataModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("version")]
      public int Version { get; set; }

      [JsonProperty("data")]
      public JToken Data { get; set; }
    }

    public class MediaModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("data")]
      public JObject Data { get; set; }

      [JsonProperty("index")]
      public int Index { get; set; }

      [JsonProperty("position")]
      public int[] Position { get; set; }
    }

    public class ContentModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("type")]
      public string Type { get; set; }

      [JsonProperty("url")]
      public string Url { get; set; }

      [JsonProperty("path")]
      public string Local { get; set; }

      [JsonIgnore]
      public Uri Path { get; set; }

      [JsonProperty("md5")]
      public string Md5 { get; set; }

      [JsonProperty("size")]
      public long Size { get; set; }
    }

    public class ScheduleModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("weekDays")]
      public Dictionary<int, string> WeekDays { get; set; }

      [JsonProperty("timestamps")]
      public Dictionary<long, string> Timestamps { get; set; }
    }

    public class PlaylistModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("schedules")]
      public int[] Schedules { get; set; }

      [JsonProperty("audios")]
      public int[] Audios { get; set; }

      [JsonProperty("main")]
      public MainModel[] Main { get; set; }

      [JsonProperty("widgets")]
      public long[] Widgets { get; set; }
    }

    public class MainModel
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("duration")]
      public double Duration { get; set; }

      [JsonProperty("audio")]
      public int? PlayAudio { get; set; }

      [JsonProperty("content")]
      public long Content { get; set; }

      [JsonProperty("filter")]
      public int Filter { get; set; }

      [JsonIgnore]
      public bool[] DaysOfWeek { get; set; } = new bool[7];

      [JsonIgnore]
      public int From { get; set; }

      [JsonIgnore]
      public int To { get; set; }
    }
  }
}
