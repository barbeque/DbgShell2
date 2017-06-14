using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension
{
    public class NetThread
    {
        static List<NetThread> netThreads;
        static void InitNetThreads()
        {
            if (netThreads != null)
            {
                return;
            }
            netThreads = new List<NetThread>();
            string[] lines = Utils.GetDbgLines("!threads");
            bool isThread = false;
            foreach (string line in lines)
            {
                if (isThread)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        NetThread thread = new NetThread();
                        thread.Populate(line);
                        netThreads.Add(thread);
                    }
                } else if (line.Contains("ThreadOBJ"))
                {
                    isThread = true;
                }
            }
        }
        public static List<NetThread> NetThreads
        {
            get
            {
                InitNetThreads();
                return netThreads;
            }
        }


        public int Id = -1;
        public bool GCEnabled = false;
        public bool Finalizer = false;
        public bool GC = false;
        public bool Exception = false;

        public NetThread()
        {
        }

        public void Populate(string line)
        {
            String[] tokens = line.Split(' ');

            if (tokens[2] != "")
                Id = Int32.Parse(tokens[2]);

            if (line.Contains("GC")) GC = true;
            if (line.Contains("Finalizer")) Finalizer = true;
            if (line.Contains("Exception")) Exception = true;
            if (line.Contains("Enabled")) GCEnabled = true;
        }
    }
}
