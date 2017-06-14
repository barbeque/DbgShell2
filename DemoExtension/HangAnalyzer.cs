using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension
{
    public static class HangAnalyzer
    {
        private static void AnalyzeGC()
        {
            Output.WriteLine("\n____________________________________________________________________________________________________________________");
            Output.WriteLine("\n GC RELATED INFORMATION ");
            Output.WriteLine("____________________________________________________________________________________________________________________\n");
            Output.Write("The following threads are GC threads:\n");
            Utils.OutputStacks(CallStack.FindNativeCriteria("gc_thread_stub"), true);

            Output.WriteLine("The following threads are waiting for the GC to finish:");
            Utils.OutputStacks(CallStack.FindNativeCriteria("WaitUntilGCComplete"), true);

            if (CallStack.FindNativeCriteria("SysSuspendForGC").Count > 0)
            {
                Output.WriteLine("The GC is working on suspending threads to continue with garbage collection");
                Output.WriteLine("The following threads can't be suspended because preemptive GC is disabled");
                Utils.OutputStacks(NetThread.NetThreads.FindAll(thread => !thread.GCEnabled), false);
            }
            if (CallStack.FindNativeCriteria("WaitForFinalizerEvent").Count == 0)
            {
                Output.Write("The finalizer (");
                int finalizerId = NetThread.NetThreads.Find(thread => thread.Finalizer).Id;
                Utils.OutputStack(finalizerId, false);
                Output.WriteLine(") is blocked.");
            }
            else
            {
                Output.Write("The finalizer is not blocked");
            }
            Output.WriteLine("\nBLOG POSTS ABOUT GC RELATED ISSUES");
            Output.WriteLine("===================================");
            Output.WriteLine("GC-LoaderLock Deadlock \t\t http://blogs.msdn.com/tess/archive/2007/03/12/net-hang-case-study-the-gc-loader-lock-deadlock-a-story-of-mixed-mode-dlls.aspx");
            Output.WriteLine("High CPU in GC \t\t\t http://blogs.msdn.com/tess/archive/2006/06/22/643309.aspx");
            Output.WriteLine("High CPU in GC (Viewstate) \t http://blogs.msdn.com/tess/archive/2006/11/24/asp-net-case-study-bad-perf-high-memory-usage-and-high-cpu-in-gc-death-by-viewstate.aspx");
            Output.WriteLine("Blocked finalizer \t\t http://blogs.msdn.com/tess/archive/2006/03/27/561715.aspx");
        }
        private static void AnalyzeLocks()
        {
            Output.WriteLine("\n____________________________________________________________________________________________________________________");
            Output.WriteLine("\n INFORMATION ABOUT LOCKS AND CRITICAL SECTIONS ");
            Output.WriteLine("____________________________________________________________________________________________________________________\n");

            Output.WriteLine("The following threads are spinning waiting to enter a .NET Lock:");
            Utils.OutputStacks(CallStack.FindNativeCriteria("JIT_MonTryEnter"), true);

            Output.WriteLine("The following threads are waiting to enter a .NET Lock:\t(TIP! Run !sos.syncblk to find out who the owner is)\n\t");
            Utils.OutputStacks(CallStack.FindNativeCriteria("JITutil_MonContention"), true);

            Output.WriteLine("The following threads are waiting for a critical section:\t(TIP! Run !sieextPub.critlist to find out who the owner is");
            Utils.OutputStacks(CallStack.FindNativeCriteria("EnterCriticalSection"), true);

            Output.WriteLine("The following threads are waiting in a WaitOne");
            Utils.OutputStacks(CallStack.FindNativeCriteria("WaitOne"), true);

            Output.WriteLine("The following threads are waiting in a WaitMultiple");
            Utils.OutputStacks(CallStack.FindNativeCriteria("WaitMultiple"), true);

            Output.WriteLine("\nBLOG POSTS ABOUT LOCKS AND CRITICAL SECTIONS");
            Output.WriteLine("==============================================");
            Output.WriteLine("WaitOne and WebService calls \t http://blogs.msdn.com/tess/archive/2006/02/23/537681.aspx");
            Output.WriteLine("Locks and Critical sections \t http://blogs.msdn.com/tess/archive/2006/01/09/510773.aspx");

        }
        private static void AnalyzeExternalProcesses()
        {
            Output.WriteLine("\n____________________________________________________________________________________________________________________");
            Output.WriteLine("\n INFORMATION ABOUT THREADS WAITING ON EXTERNAL PROCESSES ");
            Output.WriteLine("____________________________________________________________________________________________________________________\n");

            Output.WriteLine("The following threads are waiting in a Socket.Receive");
            Utils.OutputStacks(CallStack.FindClrCriteria("Socket.Receive"), false);

            Output.WriteLine("The following threads are waiting in a SendReceive2 (COM Call):\t(TIP! Run !sieextPub.comcalls to find out where the calls are going)");
            Utils.OutputStacks(CallStack.FindNativeCriteria("SendReceive2"), true);

            Output.WriteLine("The following threads are waiting in a GetToSTA (Waiting for an STA thread):\t(TIP! Run !sieextPub.comcalls to find out where the calls are going)");
            Utils.OutputStacks(CallStack.FindNativeCriteria("GetToSTA"), true);
        }
        public static void Show()
        {
            Output.WriteLine("\n\n\n\n====================================================================================================================");
            Output.WriteLine("                   Hang Analysis                          ");
            Output.WriteLine("====================================================================================================================");

            AnalyzeGC();
            AnalyzeLocks();
            AnalyzeExternalProcesses();

            Output.WriteLine("\nBLOG POSTS ABOUT DEBUGGING HANGS");
            Output.WriteLine("==============================================");
            Output.WriteLine("Hang debugging walkthrough\t\t http://blogs.msdn.com/tess/archive/2006/10/16/net-hang-debugging-walkthrough.aspx");
            Output.WriteLine("Things to ignore when you're debugging a hang\t http://blogs.msdn.com/tess/archive/2007/04/02/things-to-ignore-when-debugging-an-asp-net-hang-update-for-net-2-0.aspx\n\n\n\n");

        }
    }
}
