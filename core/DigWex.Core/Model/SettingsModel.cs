namespace DigWex.Api.Model
{
  public class SettingsModel
  {
    public BrightnessscheduleModel[] brightnessSchedule { get; set; }
    public int[] rebootSchedule { get; set; }
    public FillModel fill { get; set; }
    public WindowModel window { get; set; }
    public int rotation { get; set; }


    public class FillModel
    {
      public bool image { get; set; }
      public bool video { get; set; }
    }

    public class WindowModel
    {
      public int x { get; set; }
      public int y { get; set; }
      public int width { get; set; }
      public int height { get; set; }
      public bool fullscreen { get; set; }
    }

    public class BrightnessscheduleModel
    {
      public int time { get; set; }
      public double value { get; set; }
    }

  }

}
