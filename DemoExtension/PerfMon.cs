using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension
{
    [DbgClass]
    public static class PerfMon
    {
        static string PrivatePerfCounters = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
                return;
            isInitialized = true;
            foreach (string token in Utils.GetDbgTokens("lm1m"))
            {
                if ((token == "mscorsvr") || (token == "mscorwks"))
                {
                    PrivatePerfCounters = Utils.GetDbgTokens("?"+token+"!PerfCounters::m_pPrivatePerf")[4];
                    break;
                }
            }
        }

        static string GetValue(int offset)
        {
            string cmd = "?poi(poi(" + PrivatePerfCounters + ")+" + offset.ToString() + "*@$ptrsize)";
            return Utils.GetDbgTokens(cmd)[2];
        }
        [DbgMethod]
        public static void ShowGC()
        {
            Init();
            if (string.IsNullOrEmpty(PrivatePerfCounters))
            {
                Output.WriteLine("Not a .Net application");
                return;
            }
            Output.WriteLine("NET GC Counters\n");
            Output.WriteLine("GenCollection 0          \t= " + GetValue(1));
            Output.WriteLine("GenCollection 1          \t= " + GetValue(2));
            Output.WriteLine("GenCollection 2          \t= " + GetValue(3));
            Output.WriteLine("PromotedMemory           \t= " + GetValue(4));
            Output.WriteLine("PromotedMemory 1         \t= " + GetValue(5));
            Output.WriteLine("PromotedFinalizationMem 0\t= " + GetValue(6));
            Output.WriteLine("Process ID               \t= " + GetValue(7));
            Output.WriteLine("GenHeapSize 0            \t= " + GetValue(8));
            Output.WriteLine("GenHeapSize 1            \t= " + GetValue(9));
            Output.WriteLine("GenHeapSize 2            \t= " + GetValue(10));
            Output.WriteLine("TotalCommittedBytes      \t= " + GetValue(11));
            Output.WriteLine("TotalReservedBytes       \t= " + GetValue(12));
            Output.WriteLine("LargeObjectSize          \t= " + GetValue(13));
            Output.WriteLine("SurviveFinalize          \t= " + GetValue(14));
            Output.WriteLine("Handles                  \t= " + GetValue(15));
            Output.WriteLine("Alloc                    \t= " + GetValue(16));
            Output.WriteLine("LargeAlloc               \t= " + GetValue(17));
            Output.WriteLine("InducedGCs               \t= " + GetValue(18));
            Output.WriteLine("TimeInGC                 \t= " + GetValue(19));
            Output.WriteLine("TimeInGCBase             \t= " + GetValue(20));
            Output.WriteLine("PinnedObjects            \t= " + GetValue(21));
            Output.WriteLine("SinkBlocks               \t= " + GetValue(22));
        }
    }
}
