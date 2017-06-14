using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension.Http
{
    class HttpRequest
    {
        static string httpVerbOffset = string.Empty;
        static string pathOffset = string.Empty;
        static string queryStringTextOffset = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            string eeClass = Utils.Name2EE("System.Web.dll", "System.Web.HttpRequest");
            httpVerbOffset = Utils.GetOffset(eeClass, "_httpVerb");
            pathOffset = Utils.GetOffset(eeClass, "_path");
            queryStringTextOffset = Utils.GetOffset(eeClass, "_queryStringText");
        }


        string address;
        public string Verb { get; private set; }
        public string Query { get; private set; }
        public VirtualPath Path { get; private set; }
        public HttpRequest(string address)
        {
            Init();
            this.address = address;

            string cmd = string.Format("dd {0}+{1} l1", address, httpVerbOffset);
            int code;
            int.TryParse(Utils.GetDbgTokens(cmd)[1], out code);
            switch (code)
            {
                case 2: 
                    Verb = "GET";
                    break;
                case 5:
                    Verb = "POST";
                    break;
                default:
                    Verb = "Unparsed";
                    break;
            }

            Query = Utils.GetString(Utils.GetFieldAddress(address, queryStringTextOffset));

            Path = new VirtualPath(Utils.GetFieldAddress(address, pathOffset));
        }
        public void Write()
        {
            Output.AddDbgLink(address, "!do " + address);
            Output.Write("\t");
            Output.Write(Verb + "\t\t");
            Path.Write();
            if (!string.IsNullOrEmpty(Query))
            {
                Output.Write(" ?");
            }
            Output.Write(Query);
        }
    }
}
