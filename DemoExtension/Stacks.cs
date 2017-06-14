using System;
using System.Collections.Generic;
using DbgShell;

namespace DemoXtension
{
    [DbgClass]
    public static class Stacks
    {
        [DbgMethod]
        public static void FindNativePattern(string pattern)
        {
            Output.Write("Searching for pattern " + pattern + "\n");
            Utils.OutputStacks(CallStack.FindNativeCriteria(pattern), true);
        }

        [DbgMethod]
        public static void FindClrPattern(string pattern)
        {
            Output.Write("Searching for pattern " + pattern + "\n");
            Utils.OutputStacks(CallStack.FindClrCriteria(pattern), false);
        }
    }
}
