using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoXtension
{
    class HttpWorkerRequest
    {
        static string startTimeOffset = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            string eeClass = Utils.Name2EE("System.Web.dll", "System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6");
            startTimeOffset = Utils.GetOffset(eeClass, "_startTime");
        }


        string address;
        public DateTime StartTime { get; private set; }
        public HttpWorkerRequest(string address)
        {
            Init();
            this.address = address;
            string cmd = string.Format("dq {0}+{1} l1", address, startTimeOffset);
            cmd = Utils.GetDbgTokens(cmd)[1];
            cmd = string.Format("?{0}&0x3FFFFFFFFFFFFFFF", cmd);
            long ticks = 0;
            long.TryParse(Utils.GetDbgTokens(cmd)[2], out ticks);
            StartTime = new DateTime(ticks);
        }
    }
}
