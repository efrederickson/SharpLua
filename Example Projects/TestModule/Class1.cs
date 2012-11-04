using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;

namespace TestModule
{
    [LuaModule("testmodule")]
    public class TestModule
    {
        [LuaFunction]
        public static void What()
        {
            Console.WriteLine("ha");
        }

        [LuaFunction]
        // Doesn't show up - not static
        public int X()
        {
            return 9;
        }

        [LuaFunction]
        public static string GetVersion()
        {
            return "1.0 Beta";
        }
    }
}
