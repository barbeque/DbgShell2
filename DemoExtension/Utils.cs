using System;
using System.Linq;
using DbgShell;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DemoXtension.Http;

namespace DemoXtension
{
    [DbgClass]
    public static class Utils
    {
        public static string[] GetDbgLines(string command)
        {
            return Dbg.Execute(command).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        }
        public static string[] GetDbgTokens(string command)
        {
            return Dbg.Execute(command).Split(new char[] { '\r', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void OutputStack(int threadId, bool isNative)
        {
            string cmd;
            if (isNative)
            {
                cmd = "~" + threadId.ToString() + " kb";
            }
            else
            {
                cmd = "~" + threadId.ToString() + " e !clrstack";
            }
            Output.AddDbgLink(threadId.ToString(), cmd);
        }
        public static void OutputStacks(List<int> ids, bool isNative)
        {
            if (ids.Count == 0)
            {
                Output.Write("No threads found.\n");
            }
            else
            {
                Output.Write("Threads : \n");
                for (int i = 0; i < ids.Count - 1; i++)
                {
                    OutputStack(ids[i], isNative);
                    Output.Write(", ");
                }
                OutputStack(ids[ids.Count - 1], isNative);
                Output.Write("\n");
            }
        }
        public static void OutputStacks(List<CallStack> stacks, bool isNative)
        {
            List<int> ids = stacks.ConvertAll<int>(stack => stack.ThreadID);
            OutputStacks(ids, isNative);
        }
        public static void OutputStacks(List<NetThread> threads, bool isNative)
        {
            List<int> ids = threads.ConvertAll<int>(thread => thread.Id);
            OutputStacks(ids, isNative);
        }

        public static string Name2EE(string dll, string typeName)
        {
            bool found = false;
            foreach (string token in GetDbgTokens("!name2ee " + dll + " " + typeName)) 
            {
                if (found)
                {
                    return token;
                }
                if (token == "EEClass:")
                {
                    found = true;
                }
            }
            return string.Empty;
        }
        public static string Name2EE(string typeName)
        {
            return Name2EE("*", typeName);
        }
        public static string Name2MT(string dll, string typeName)
        {
            bool found = false;
            foreach (string token in GetDbgTokens("!name2ee " + dll + " " + typeName))
            {
                if (found)
                {
                    return token;
                }
                if (token == "MethodTable:")
                {
                    found = true;
                }
            }
            return string.Empty;
        }
        public static string Name2MT(string typeName)
        {
            return Name2MT("*", typeName);
        }

        public static string GetOffset(string eeClass, string memberName)
        {
            string[] tokens = GetDbgTokens("!dumpclass " + eeClass);
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == memberName)
                {
                    return tokens[i - 4];
                }
            }
            return string.Empty;
        }
        public static string GetString(string address)
        {
            bool found = false;
            foreach (string token in GetDbgTokens("!do -nofields " + address))
            {
                if (found)
                {
                    return token;
                }
                if (token == "String:")
                {
                    found = true;
                }
            }
            return string.Empty;
        }

        public static string GetFieldAddress(string objAdrress, string fieldOffset)
        {
            string cmd = string.Format("?poi({0}+{1})", objAdrress, fieldOffset);
            return GetDbgTokens(cmd)[4];
        }

        #region Contexts
        static void WriteContexts(List<HttpContext> contexts)
        {
            //Header
            Output.WriteLine("\t\tHttp Contexts");
            Output.WriteLine("HttpContext\tStartTime\t\t\tTimeOut (sec)\tHttpResponse\tCompleted\tReturnCode\tHttpRequest\tRequestType\tURL+QueryString");
            Output.Write("=========================================================================================================");
            Output.WriteLine("=============================================================================");
            foreach (HttpContext context in contexts)
            {
                context.Write();
            }
            int count = contexts.FindAll(context => context.Response.Completed).Count;
            Output.Write("\n\n");
            Output.Write(contexts.Count.ToString() + " Contexts : ");
            Output.AddScriptLink(count.ToString() + " Completed", "DbgShellUtils.Utils", "WriteCompletedContexts");
            Output.Write(", ");
            count = contexts.FindAll(context => !context.Response.Completed).Count;
            Output.AddScriptLink(count.ToString() + " Pending", "DbgShellUtils.Utils", "WritePendingContexts");
            Output.Write("\n");
            count = contexts.FindAll(context => (context.Request.Verb == "POST")).Count;
            Output.AddScriptLink(count.ToString() + " Post", "DbgShellUtils.Utils", "WritePostContexts");
            Output.Write(", ");
            count = contexts.FindAll(context => (context.Request.Verb == "GET")).Count;
            Output.AddScriptLink(count.ToString() + " Get", "DbgShellUtils.Utils", "WriteGetContexts");
            Output.Write("\n");
            Output.Write("Status Codes\tCount\n");
            Output.WriteLine("=======================");
            foreach (var group in contexts.GroupBy(context => context.Response.StatusCode))
            {
                Output.AddScriptLink(group.Key.ToString(), "DbgShellUtils.Utils", "WriteStatusCodeContext", group.Key.ToString());
                Output.Write("\t\t" + group.Count().ToString() + "\n");
            }
        }
        [DbgMethod]
        public static void WriteContexts()
        {            
            WriteContexts(HttpContext.Contexts);
        }
        public static void WriteCompletedContexts()
        {
            WriteContexts(HttpContext.Contexts.FindAll(context => context.Response.Completed));
        }
        public static void WritePendingContexts()
        {
            WriteContexts(HttpContext.Contexts.FindAll(context => !context.Response.Completed));
        }
        public static void WritePostContexts()
        {
            WriteContexts(HttpContext.Contexts.FindAll(context => (context.Request.Verb == "POST")));
        }
        public static void WriteGetContexts()
        {
            WriteContexts(HttpContext.Contexts.FindAll(context => (context.Request.Verb == "GET")));
        }
        public static void WriteStatusCodeContext(string code)
        {
            int statusCode = int.Parse(code);
            WriteContexts(HttpContext.Contexts.FindAll(context => (context.Response.StatusCode == statusCode)));
        }
        #endregion
        [DbgMethod]
        public static void HangAnalyzer()
        {
            DemoXtension.HangAnalyzer.Show();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszLongPath,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder lpszShortPath,
           uint cchBuffer);

        [DbgMethod]
        public static void DumpModules()
        {
            string dir = Path.GetDirectoryName(typeof(Utils).Assembly.Location);
            dir = Path.Combine(dir, "Modules");
            Output.WriteLine("Dumping all modules to directory " + dir);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);

            uint bufferSize = 4096;
            StringBuilder shortNameBuffer = new StringBuilder((int)bufferSize);
            GetShortPathName(dir, shortNameBuffer, bufferSize);

            dir = Path.Combine(shortNameBuffer.ToString(), "${@#ModuleName}.dll");
            Dbg.Execute("!for_each_module !savemodule ${@#Base} " + dir);
        }
    }
}
