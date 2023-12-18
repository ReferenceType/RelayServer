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
        private static long lastRequestTimeCpu;
        private static long lastRequestTimeMemory;
        private static string lastCpuUsage;
        private static string lastMemoryUsage;
        static Stopwatch sw = new Stopwatch();
        static double ProcCount;
        private static DateTime processStartTime;

        static PerformanceStatistics()
        {
            process = Process.GetCurrentProcess();
            lastProctime=process.TotalProcessorTime;
            lastRequestTimeCpu = 0;
            lastRequestTimeMemory = 0;
            ProcCount = Convert.ToDouble(Environment.ProcessorCount);
            processStartTime = process.StartTime;
            sw.Start();

        }

       
        public static string GetCpuUsage()
        {
            long elapsed = sw.ElapsedMilliseconds;
            long deltaTms = elapsed - lastRequestTimeCpu;
            if (deltaTms < 900)
            {
                return lastCpuUsage;
            }
            var currentProcessorTime = process.TotalProcessorTime;
            double CPUUsage = (currentProcessorTime.TotalMilliseconds - lastProctime.TotalMilliseconds) / (deltaTms * ProcCount);
            lastRequestTimeCpu = elapsed;
            lastProctime = currentProcessorTime;
            lastCpuUsage = (CPUUsage * 100).ToString("N3") + "%";
            return lastCpuUsage;

        }

        public static string GetMemoryUsage()
        {
            const double f = 1024.0 * 1024.0;
            return (Environment.WorkingSet / f).ToString("N3") + "MB";

            long elapsed = sw.ElapsedMilliseconds;
            long deltaTms = elapsed - lastRequestTimeMemory;
            if (deltaTms < 900)
            {
                return lastMemoryUsage;
            }
            lastRequestTimeMemory=elapsed;
            process.Refresh();
            lastMemoryUsage = (process.WorkingSet64 / f).ToString("N3") + "MB";
            return  lastMemoryUsage;

        }
    }
}
