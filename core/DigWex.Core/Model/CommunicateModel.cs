using System.Collections.Generic;
using Newtonsoft.Json;
using DigWex.Converters;
using System;

namespace DigWex.Model
{
  public class CommunicateModel
  {
    [JsonProperty("commands_acknowledge")]
    public List<int> CommandsAcknowledge { get; set; }

    [JsonProperty("power")]
    public bool? Power { get; set; }

    [JsonProperty("sensors_values")]
    public Dictionary<string, object> SensorsValues { get; set; }

    [JsonProperty("synchronization")]
    public SynchronizationModel Synchronization { get; set; }

    [JsonProperty("telemetry")]
    public TelemetryModel Telemetry { get; set; }

    [JsonProperty("ulogs")]
    public List<object> Ulogs { get; set; }
  }

  public class SynchronizationModel
  {
    [JsonProperty("device_data_id")]
    public int DeviceDataId { get; set; }

    [JsonProperty("progress")]
    public float Progress { get; set; }
  }

  public class TelemetryModel
  {
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("time")]
    [JsonConverter(typeof(CustomDateFormat))]
    public DateTime Time { get; set; }

    [JsonProperty("disk")]
    public Disc Disk { get; set; }

    [JsonProperty("software_version")]
    public string SoftwareVersion { get; set; }

    [JsonProperty("resolution")]
    public int[] Resolution { get; set; }

    [JsonProperty("network_ip")]
    public string NetworkIp { get; set; }


    [JsonProperty("network_mac")]
    public string NetworkMac { get; set; }

    //[JsonProperty("cpu")]
    //public double Cpu { get; set; }

    //[JsonProperty("memory")]
    //public SortedDictionary<string, AvaliableTotal> Memory { get; set; }

    //[JsonProperty("dma")]
    //public SortedDictionary<string, AvaliableTotal> Dma { get; set; }
  }

  public class Disc
  {
    [JsonProperty("root")]
    public AvaliableTotal Root { get; set; }

    [JsonProperty("data")]
    public AvaliableTotal Data { get; set; }

    [JsonProperty("tmp")]
    public AvaliableTotal Tmp { get; set; }
  }

  public class AvaliableTotal
  {
    [JsonProperty("available")]
    public long Available { get; set; }

    [JsonProperty("total")]
    public long Total { get; set; }
  }
}
