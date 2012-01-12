/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 1/4/2012
 * Time: 3:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SharpLua;
using SharpLua.LuaTypes;

namespace CSharpExampleProject
{
    class Program
    {
        public static void Main(string[] cmd_args_1938475092347027340582734) // random name doesnt interfere with my variables
        {
            // Create a global environment
            LuaTable t = LuaRuntime.CreateGlobalEnviroment();
            
            // to add an item, dont use AddValue, it sticks it into the back
            // instead, use SetNameValue
            t.SetNameValue("obj", new LuaString("sample object"));
            
            // we can set the MetaTable of item "obj", but first we must get it from
            // the global environment:
            LuaValue val = t.GetValue("obj");
            
            // We can then print "val"
            Console.WriteLine(val.ToString());   // --> sample object
            
            // to register methods, use the Register function (using Addresses or delegates)
            t.Register("samplefunc", delegate(LuaValue[] args)
                       {
                           return new LuaNumber(100);
                       });
            
            // To run Lua, use the Run function in LuaRuntime 
            // we pass "t" as the specified environment, otherwise it will 
            // create a new environment to run in.
            LuaRuntime.Run("print(\"obj:\", obj, \"\nsamplefunc:\", samplefunc())", t);
            
            // we can also call .NET methods using Lua-created .NET object
            // such as:
            LuaRuntime.Run("obj2 = script.create(\"CSharpExampleProject.TestClass\")", t);
            LuaRuntime.Run("print(\"testint:\", obj2.testint, \"TestFunc:\", obj2.TestFunc())", t);
            
            // the reason for this is because script.create returns an advanced Userdata with
            // metatables that check any indexing or setting and map them to the .NET object
            // if it doesn't find it, it throws an error
            //LuaRuntime.Run("obj2.ThisValueDoesntExistInDotNet = \"hey\"", t);
            // the same value was printed twice, with different functions each time, proving its not actually there:
            //Console.WriteLine(LuaRuntime.Run("return \"Sample attemption at creating an object: \" .. tostring(obj2.ThisValueDoesntExistInDotNet)", t));
            // Console.WriteLine was used to show that you can print the returned value of executed code
            //LuaRuntime.Run("print(obj2.ThisValueDoesntExistInDotNet)", t);
            
            // Lua can also create "classes"
            LuaRuntime.Run("c = class()", t);
            Console.WriteLine(LuaRuntime.Run("return c", t));
            
            // You can also call functions defined in #Lua
            LuaFunction f = LuaRuntime.Run("return function() print\"a function says hai\" end", t) as LuaFunction;
            f.Invoke(new LuaValue[] { });
            
            // Let you see the output
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
    
    public class TestClass
    {
        public LuaValue testint = new LuaNumber(100);
        public string TestFunc()
        {
            return "Testing...";
        }
    }
    
    public class Test2
    {
        public List<string> Values;
        
        public Test2()
        {
            Values = new List<string>();
            for (int i = 1; i <= 60; i++)
                Values.Add(i.ToString());
        }
        
        public string this[int index]
        {
            get
            {
                return Values[index];
            }
        }
    }
}