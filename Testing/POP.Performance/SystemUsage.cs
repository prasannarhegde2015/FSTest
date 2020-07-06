using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace POP.Performance
{
    public class SystemUsage : APIPerfTestBase
    {
        public static object getCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            dynamic firstValue = cpuCounter.NextValue();
            Thread.Sleep(1000);
            dynamic secondValue = cpuCounter.NextValue();

            return secondValue;
        }

        public static object getRAM()
        {
            PerformanceCounter ramCounter = new PerformanceCounter();
            ramCounter.CategoryName = "Memory";
            ramCounter.CounterName = "Available MBytes";

            dynamic firstValue = ramCounter.NextValue();
            Thread.Sleep(1000);
            dynamic secondValue = ramCounter.NextValue();

            return secondValue;
        }

        public static System.Timers.Timer timer;

        public void GetSystemUsage()
        {
            timer = new System.Timers.Timer(15000);
            timer.Elapsed += Event;
            timer.AutoReset = true;
            timer.Enabled = true;
            Console.WriteLine("Press any key to exit...");
            Console.WriteLine("time, CPU Usage, RAM Usage");
            Console.ReadLine();
        }

        public static void Event(object source, ElapsedEventArgs e)
        {
            object CPU = getCPUCounter();
            object RAM = getRAM();
            Console.WriteLine(e.SignalTime + ", " + CPU.ToString() + ", " + RAM.ToString());
            WriteSystemUsageLogFile(e.SignalTime.ToString() + ", " + CPU.ToString() + ", " + RAM.ToString());
        }
    }
}