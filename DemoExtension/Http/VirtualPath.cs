using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbgShell;

namespace DemoXtension.Http
{
    class VirtualPath
    {
        static string virtualPathOffset = string.Empty;
        static bool isInitialized = false;
        static void Init()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            string eeClass = Utils.Name2EE("System.Web.dll", "System.Web.VirtualPath");
            virtualPathOffset = Utils.GetOffset(eeClass, "_virtualPath");
        }


        string address;
        public string Path { get; private set; }
        public VirtualPath(string address)
        {
            Init();
            this.address = address;

            Path = Utils.GetString(Utils.GetFieldAddress(address, virtualPathOffset));
        }
        public void Write()
        {
            Output.Write(Path);
        }
    }
}
