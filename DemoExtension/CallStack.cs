using System;
using System.Collections.Generic;
using DbgShell;

namespace DemoXtension
{
    public class CallStack
    {
        static List<CallStack> nativeStacks;
        static void InitNativeStacks()
        {
            if (nativeStacks != null)
            {
                return;
            }
            nativeStacks = new List<CallStack>();
            string[] lines = Utils.GetDbgLines("~* kb");
            string prevLine = string.Empty;
            CallStack stack = null;
            foreach (string line in lines)
            {
                if ((stack != null) && (!string.IsNullOrEmpty(line)))
                {
                    stack.AddFrame(line);
                }
                else
                {
                    stack = null;
                }
                if (line.Contains("RetAddr"))
                {
                    string[] tokens = prevLine.Split(' ');
                    int id = 0;
                    if (tokens[1] != "") id = Int32.Parse(tokens[1]);
                    else if (tokens[2] != "") id = Int32.Parse(tokens[2]);
                    else id = Int32.Parse(tokens[3]);

                    stack = new CallStack(id);
                    nativeStacks.Add(stack);
                }
                prevLine = line;
            }
        }
        public static List<CallStack> NativeStacks
        {
            get
            {
                InitNativeStacks();
                return nativeStacks;
            }
        }

        static List<CallStack> netStacks;
        static void InitNetStacks()
        {
            if (netStacks != null)
            {
                return;
            }
            netStacks = new List<CallStack>();
            string[] lines = Utils.GetDbgLines("~* e !clrstack");
            CallStack stack = null;
            foreach (string line in lines)
            {
                if ((stack != null) && (!string.IsNullOrEmpty(line)))
                {
                    if (line.Contains("OS Thread Id"))
                    {
                        if (stack.IsManaged)
                        {
                            netStacks.Add(stack);
                        }
                        stack = null;
                    }
                    else if (line.Contains("Failed to start stack walk"))
                    {
                        stack.AddFrame("No managed stack");
                        netStacks.Add(stack);
                        stack = null;
                    }
                    else if (line.Contains("Unable to walk"))
                    {
                        stack = null;
                    }
                    else
                    {
                        stack.AddFrame(line);
                    }
                }
                else
                {
                    stack = null;
                }
                if (stack == null)
                {
                    int start = line.IndexOf('(');
                    int end = line.IndexOf(')');
                    if ((start >= 0) && (end > start))
                    {
                        int id = Int32.Parse(line.Substring(start + 1, end - start - 1));
                        stack = new CallStack(id);
                        stack.IsManaged = true;
                    }
                }

            }
        }
        public static List<CallStack> NetStacks
        {
            get
            {
                InitNetStacks();
                return netStacks;
            }
        }

        public static List<CallStack> FindNativeCriteria(string pattern)
        {
            return NativeStacks.FindAll(stack => stack.FitsCriteria(pattern));
        }

        public static List<CallStack> FindClrCriteria(string pattern)
        {
            return NetStacks.FindAll(stack => stack.FitsCriteria(pattern));
        }

        public int ThreadID = -1;
        public List<string> Frames;
        public bool IsManaged;

        public CallStack(int ThreadID)
        {
            this.ThreadID = ThreadID;
            Frames = new List<string>();
        }

        public void AddFrame(string Frame)
        {
            Frames.Add(Frame);
        }

        public bool FitsCriteria(string criteria)
        {
            bool fits = false;
            foreach (string frame in Frames)
            {
                if (frame.Contains(criteria))
                {
                    fits = true;
                    break;
                }
            }
            return fits;
        }
    }
}
