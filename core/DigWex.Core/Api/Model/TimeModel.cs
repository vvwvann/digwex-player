using Newtonsoft.Json;
using System;

namespace DigWex.Api.Model
{
  public class TimeModel
  {
    public class Response
    {
      [JsonProperty("utc")]
      public DateTime Utc { get; set; }

      [JsonProperty("offset")]
      public int Offset { get; set; }
    }
  }
}
