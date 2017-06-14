using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using DbgShell;

namespace DbgShellUI
{
    static class ScriptsApi
    {
        static AppDomain compiledDomain;
        static Runner init;

        public static void Load()
        {
            Unload();
            compiledDomain = AppDomain.CreateDomain("Scripts Domain");

            init = (Runner)compiledDomain.CreateInstance("DbgShell", "DbgShell.Runner").Unwrap();
            string currentDir = Path.GetDirectoryName(typeof(ScriptsApi).Assembly.Location);
            init.Start(Dbg.Process, currentDir);
        }

        public static void Unload()
        {
            if (compiledDomain != null)
            {
                AppDomain.Unload(compiledDomain);
            }
            compiledDomain = null;
        }

        public static IEnumerable<string> GetTypes()
        {
            return init.GetTypes();
        }

        public static IEnumerable<string> GetMethods(string type)
        {
            return init.GetMethods(type);
        }

        public static string Run(string typeName, string methodName, params object[] list)
        {
            if (compiledDomain == null)
            {
                return string.Empty;
            }
            try
            {
                return init.Run(typeName, methodName, list);
            }
            catch (TargetInvocationException e)
            {
                Exception inner = e.InnerException;
                throw inner;
            }

        }
    }
}
