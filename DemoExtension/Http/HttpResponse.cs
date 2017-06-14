using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension.Http
{
    class HttpResponse
    {
        static string completedOffset = string.Empty;
        static string statusCodeOffset = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            string eeClass = Utils.Name2EE("System.Web.dll", "System.Web.HttpResponse");
            completedOffset = Utils.GetOffset(eeClass, "_completed");
            statusCodeOffset = Utils.GetOffset(eeClass, "_statusCode");
        }

        string address;
        public bool Completed { get; private set; }
        public int StatusCode { get; private set; }
        public HttpResponse(string address)
        {
            Init();
            this.address = address;

            string cmd = string.Format("dq {0}+{1}", address, completedOffset);
            Completed = !Utils.GetDbgTokens(cmd)[1].EndsWith("0");

            cmd = string.Format("?poi({0}+{1})", address, statusCodeOffset);
            int code;
            int.TryParse(Utils.GetDbgTokens(cmd)[2], out code);
            StatusCode = code;
        }

        public void Write()
        {
            Output.AddDbgLink(address, "!do " + address);
            Output.Write("\t");
            string completed = Completed ? "Yes" : "No";
            Output.Write(completed + "\t\t");
            Output.Write(StatusCode.ToString() + "\t\t");
        }
    }
}
