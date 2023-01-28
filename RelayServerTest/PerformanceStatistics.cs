using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer
{
    internal class PerformanceStatistics
    {
        private static Process process;
        private static TimeSpan lastProctime;
        private static DateTime lastRequestTimeCpu;
        private static DateTime lastRequestTimeMemory;
        private static string lastCpuUsage;
        private static string lastMemoryUsage;

        static PerformanceStatistics()
        {
            process = Process.GetCurrentProcess();
            lastProctime=process.TotalProcessorTime;
            lastRequestTimeCpu = DateTime.Now;
            lastRequestTimeMemory = DateTime.Now;
        }
        public static string GetCpuUsage()
        {
            if((DateTime.Now - lastRequestTimeCpu).TotalMilliseconds < 500)
            {
                return lastCpuUsage;
            }
            var currentProcessorTime = process.TotalProcessorTime;
            var currentTimeStamp = DateTime.Now;
            double CPUUsage = (currentProcessorTime.TotalMilliseconds - lastProctime.TotalMilliseconds) / currentTimeStamp.Subtract(lastRequestTimeCpu).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
            lastRequestTimeCpu = currentTimeStamp;
            lastProctime = currentProcessorTime;
            lastCpuUsage = (CPUUsage * 100).ToString("N3") + "%";
            return lastCpuUsage;

        }

        public static string GetMemoryUsage()
        {
            if ((DateTime.Now - lastRequestTimeMemory).TotalMilliseconds < 500)
            {
                return lastMemoryUsage;
            }
            lastRequestTimeMemory=DateTime.Now;
            //process = Process.GetCurrentProcess();
            process.Refresh();
            const double f = 1024.0 * 1024.0;
            lastMemoryUsage = (process.WorkingSet64 / f).ToString("N3") + "MB";
            return  lastMemoryUsage;

        }
    }
}
