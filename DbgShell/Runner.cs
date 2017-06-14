using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace DbgShell
{
    public class Runner: MarshalByRefObject
    {
        private string currentDir;
        public void Start(Process process, string assemblyDir)
        {
            Dbg.Init(process);
            Output.Clear();
            foreach (string fileName in Directory.GetFiles(assemblyDir, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(fileName);
                }
                catch { }
            }
            currentDir = assemblyDir;
        }

        public List<string> GetTypes()
        {
            List<string> types = new List<string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.Location.StartsWith(currentDir, StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.GetCustomAttributes(typeof(DbgClassAttribute), true).Length > 0)
                        {
                            types.Add(type.FullName);
                        }
                    }
                }
            }
            return types;
        }

        public List<string> GetMethods(string typeName)
        {
            List<string> methods = new List<string>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (method.GetCustomAttributes(typeof(DbgMethodAttribute), true).Length > 0)
                        {
                            methods.Add(method.Name);
                        }
                    }
                }
            }
            return methods;
        }

        private static object GetTarget(MethodInfo method)
        {
            if (method.IsStatic)
            {
                return null;
            }
            else
            {
                return method.DeclaringType.Assembly.CreateInstance(method.DeclaringType.FullName);
            }
        }

        public string Run(string typeName, string methodName, params object[] list)
        {
            Output.Clear();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    MethodInfo method = type.GetMethod(methodName);
                    if (method != null)
                    {
                        if (method.GetParameters().Length > 0)
                        {
                            method.Invoke(GetTarget(method), list);
                        }
                        else
                        {
                            method.Invoke(GetTarget(method), null);
                        }
                    }
                }
            }
            return Output.ToOutputString();
        }

        public string ToOutputString()
        {
            return Output.ToOutputString();
        }
    }
}
