using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace DbgShell
{
    public static class Dbg
    {
        private static Process process;

        public static void Init(Process aProcess)
        {
            if (process == null)
                process = aProcess;
        }

        static string Execute(string command, bool removeContext)
        {
            string doneCommand = "{DC78914A-7AD6-4155-9E2F-BBCB03E14657}";
            StreamWriter writer = process.StandardInput;
            writer.WriteLine(command);
            writer.WriteLine(string.Format(".printf \"{0}\\n\"", doneCommand));
            StringBuilder result = new StringBuilder();
            string line = process.StandardOutput.ReadLine();
            while (!line.EndsWith(doneCommand))
            {
                result.AppendLine(line);
                line = process.StandardOutput.ReadLine();
            }
            line = line.Substring(0, line.Length - (doneCommand.Length + 7));
            result.Append(line);
            if (removeContext)
            {
                result.Remove(0, 7);
            }
            return result.ToString();
        }

        public static string Execute(string command)
        {
            return Execute(command, true);
        }

        public static string Context
        {
            get { return Execute(".echo", false); }
        }

        public static Process Process
        {
            get { return process; }
        }
    }
}
