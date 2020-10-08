using System;
using System.Collections.Generic;
using DigWex.Extensions;
using static DigWex.Api.Model.PackageModel;

namespace DigWex
{
  public class ScheduleCondition
  {
    const int N = 1440;
    public Dictionary<long, bool[]> _timestamps = new Dictionary<long, bool[]>();
    public bool[][] _week = new bool[7][];


    public ScheduleCondition(List<ScheduleModel> schedules, PlaylistModel playlist)
    {
      for (int i = 0; i < schedules.Count; i++) {
        if (schedules[i].WeekDays != null) {
          foreach (var item in schedules[i].WeekDays) {
            if (item.Value == null) continue;
            if (_week[item.Key] == null)
              _week[item.Key] = new bool[N];

            Decompress(item.Value, out bool[] array);
            for (int j = 0; j < N; j++) {
              _week[item.Key][j] |= array[j];
            }
          }
        }
        if (schedules[i].Timestamps != null) {
          foreach (var item in schedules[i].Timestamps) {
            if (item.Value == null) continue;
            Decompress(item.Value, out bool[] array);
            if (_timestamps.TryGetValue(item.Key, out bool[] val)) {
              for (int j = 0; j < N; j++) {
                array[j] |= val[j];
              }
            }
            _timestamps[item.Key] = array;
          }
        }
      }

      ItemDecompress(playlist);
      Modify(playlist.Main);
    }

    private void Decompress(string s, out bool[] array)
    {
      array = new bool[N];
      int j = 0;

      int num = 1, index = 0;
      for (int i = 0; i < s.Length; i++) {
        num = 0;
        while (Char.IsDigit(s[i])) {
          num = num * 10 + s[i++] - '0';
        }

        if (s[i] >= '#' && s[i] <= ',')
          index = s[i] - '#';
        else if (s[i] >= 'A' && s[i] <= 'Z')
          index = s[i] - 'A' + 10;
        else if (s[i] >= 'a' && s[i] <= 'z')
          index = s[i] - 'a' + 36;
        else if (s[i] == '!')
          index = 62;
        else if (s[i] == '@')
          index = 63;

        num = num == 0 ? 1 : num;

        while (num-- > 0) {
          string bin = Convert.ToString(index, 2);
          int n = bin.Length;
          while (n++ < 6)
            array[j++] = false;
          for (int k = 0; k < bin.Length; k++) {
            array[j++] = bin[k] != '0';
          }
        }
      }
    }

    public bool Exist(DateTime time)
    {
      long unix = time.Date.UnixTime();
      int res = (int)time.TimeOfDay.TotalMinutes;
      if (_timestamps.TryGetValue(unix, out bool[] value))
        return value[res];

      int day = (int)time.DayOfWeek;
      if (_week[day] != null)
        return _week[day][res];
      return false;
    }

    private void Modify(MainModel[] arr)
    {
      for (int i = 0; i < N; i++) {
        for (int j = 0; j < 7; j++) {
          bool ok = true;
          foreach (var item in arr) {
            if (item.DaysOfWeek[j]) {
              if (i >= item.From && i <= item.To) {
                ok = false;
                break;
              }
            }
          }
          if (ok) {
            _week[j][i] = false;
          }
        }
      }
    }

    public void ItemDecompress(PlaylistModel playlist)
    {
      foreach (var item in playlist.Main) {
        Console.WriteLine("filter: " + item.Filter);
        int bin = item.Filter;

        int mask11 = 0x7ff;
        item.From = bin & mask11;
        bin >>= 11;
        item.To = bin & mask11;
        bin >>= 11;

        int p = 1;
        for (int i = 0; i < 7; i++) {
          item.DaysOfWeek[i] = (p & bin) != 0;
          p <<= 1;
        }
      }
    }
  }
}