using System;
using System.Diagnostics;

namespace DigWex.Helpers
{
    public class PerformanceInfo
    {
        //private readonly PerformanceCounter _cpuCounter;
        //private readonly PerformanceCounter _ramCounter;

        public PerformanceInfo()
        {
            //_cpuCounter = new PerformanceCounter();
            //_ramCounter = new PerformanceCounter();
        }

        public float? GetCurrentCpuUsage()
        {
            try
            {
                //return _cpuCounter.NextValue();
            }
            catch { }
            return null;
        }

        public float? GetAvailableRAM()
        {
            try
            {
               // return _ramCounter.NextValue();
            }
            catch { }
            return null;
        }

        public void GetIpAddress()
        {

        }

        public bool InitPerfomance()
        {
            try
            {
                //_cpuCounter.CategoryName = "Processor";
                //_cpuCounter.CounterName = "% Processor Time";
                //_cpuCounter.InstanceName = "_Total";
                //_ramCounter.CategoryName = "Memory";
                //_ramCounter.CounterName = "Available MBytes";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
    }
}
