using System;

namespace DigWex.Extensions
{
  public static class DateTimeExtension
  {
    public static long UnixTime(this DateTime time)
    {
      return (long)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
  }
}
