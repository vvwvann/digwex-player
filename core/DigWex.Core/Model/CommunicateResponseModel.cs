using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DigWex.Model
{
  public class ComunicateServerModel
  {
    public int commands { get; set; }

    public Dictionary<int, JToken> extra { get; set; }
  }

  public class ComunicateClientModel
  {
    public bool display { get; set; }

    public Synchronization synchronization { get; set; }

    public Telemetry telemetry { get; set; }

    public class Synchronization
    {
      public int packageId { get; set; }

      public float progress { get; set; }
    }

    public class Telemetry
    {
      public string version { get; set; }

      public DateTime datetime { get; set; } = DateTime.Now;

      public long[] drive { get; set; }

      public int[] resolution { get; set; }

      public string hardwareModel { get; set; }

      public string hardwareSerial { get; set; }

      public string osVersion { get; set; }

      public string networkMac { get; set; }

      public long networkIp { get; set; }
    }
  }
}
