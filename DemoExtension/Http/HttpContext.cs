using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension.Http
{
    class HttpContext
    {
        static string requestOffset = string.Empty;
        static string responseOffset = string.Empty;
        static string timeoutOffset = string.Empty;
        static string wrOffset = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            string eeClass = Utils.Name2EE("System.Web.dll", "System.Web.HttpContext");
            requestOffset = Utils.GetOffset(eeClass, "_request");
            responseOffset = Utils.GetOffset(eeClass, "_response");
            timeoutOffset = Utils.GetOffset(eeClass, "_timeout");
            wrOffset = Utils.GetOffset(eeClass, "_wr");
        }

        static List<HttpContext> contexts;
        public static List<HttpContext> Contexts
        {
            get
            {
                if (contexts == null)
                {
                    InitContexts();
                }
                return contexts;
            }
        }
        static void InitContexts()
        {
            contexts = new List<HttpContext>();
            string contextMT = Utils.Name2MT("System.Web.dll", "System.Web.HttpContext");
            string cmd = string.Format("!dumpheap -mt {0} -short", contextMT);
            foreach (string address in Utils.GetDbgTokens(cmd))
            {
                if (!address.StartsWith("-"))
                {
                    contexts.Add(new HttpContext(address));
                }
            }
        }

        string address;
        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpWorkerRequest WorkerRequest { get; private set; }
        public int Timeout { get; private set; }
        public HttpContext(string address)
        {
            Init();
            this.address = address;
            
            Request = new HttpRequest(Utils.GetFieldAddress(address, requestOffset));
            
            Response = new HttpResponse(Utils.GetFieldAddress(address, responseOffset));

            WorkerRequest = new HttpWorkerRequest(Utils.GetFieldAddress(address, wrOffset));

            string cmd = string.Format("dq {0}+{1} l1", address, timeoutOffset);
            cmd = Utils.GetDbgTokens(cmd)[1];
            cmd = string.Format("?{0}/0n10000000", cmd);
            int timeout;
            int.TryParse(Utils.GetDbgTokens(cmd)[2], out timeout);
            Timeout = timeout;
        }

        public void Write()
        {
            Output.AddDbgLink(address, "!do " + address);
            Output.Write("\t");
            Output.Write(WorkerRequest.StartTime.ToString("yyyy/MM/dd hh:mm:ss:fff"));
            Output.Write("\t");
            string timeout = Timeout.ToString();
            if (timeout.Length < 6)
                timeout += "\t";
            Output.Write(timeout);
            Output.Write("\t");
            Response.Write();
            Request.Write();
            Output.Write("\n");
        }
    }
}
