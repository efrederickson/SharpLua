using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;

namespace CSharpExampleProject
{
    class Program
    {
        public static void Main(string[] cmd_args_1938475092347027340582734) // random name doesn't interfere with my variables
        {
            // Create a global environment
            LuaInterface i = LuaRuntime.GetLua();

            // to add an item, simply use the LuaInterface like a table
            i["obj"] = "sample object";

            // we can set the MetaTable of item "obj", but first we must get it from
            // the global environment:
            var val = i["obj"];

            // We can then print "val"
            Console.WriteLine(val.ToString());   // --> sample object

            // to register methods, use the Register function (using Addresses or delegates)
            i.RegisterFunction("samplefunc", null, (new Func<object>(delegate()
            {
                return 100;
            })).Method);

            // To run Lua, use the Run function in LuaRuntime 
            // we pass "t" as the specified environment, otherwise it will 
            // create a new environment to run in.
            LuaRuntime.Run(@"print(""obj:"", obj, ""\nsamplefunc:"", samplefunc())");

            // we can also call .NET methods using Lua-created .NET object
            // such as:
            LuaRuntime.Run("obj2 = clr.create(\"CSharpExampleProject.TestClass\")");
            // Notice the ':' in the method invocation (call)...
            LuaRuntime.Run("print(\"testint:\", obj2.testint, \"TestFunc:\", obj2:TestFunc())");

            // the reason for this is because clr.create returns an advanced Userdata with
            // metatables that wrap the .net object
            //LuaRuntime.Run("obj2.ThisValueDoesntExistInDotNet = \"hey\"", t);
            // the same value was printed twice, with different functions each time, proving its not actually there:
            //Console.WriteLine(LuaRuntime.Run("return \"Sample attemption at creating an object: \" .. tostring(obj2.ThisValueDoesntExistInDotNet)", t));
            // Console.WriteLine was used to show that you can print the returned value of executed code
            //LuaRuntime.Run("print(obj2.ThisValueDoesntExistInDotNet)", t);

            // You can also call functions defined in #Lua
            LuaFunction f = LuaRuntime.Run("return function() print\"a function says hai\" end")[0] as LuaFunction;
            f.Call();

            // Another class example
            i["t2"] = new Test2();
            LuaRuntime.Run("t2.Values:Sort() print(t2[1])");

            // Let you see the output
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }

    public class TestClass
    {
        public object testint = 100;
        public string TestFunc()
        {
            return "Testing...";
        }
    }

    public class Test2
    {
        public List<string> Values;
        public Dictionary<int, string> Dict;

        public Test2()
        {
            Values = new List<string>();
            Dict = new Dictionary<int, string>();
            for (int i = 1; i <= 60; i++)
            {
                Values.Add(i.ToString());
                Dict.Add(i, i.ToString() + "...");
            }
        }

        public string this[int index]
        {
            get
            {
                return Dict[index];
            }
        }
    }
}
