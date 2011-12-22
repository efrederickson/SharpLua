/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/21/2011
 * Time: 9:15 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// Description of ConsoleLib.
    /// </summary>
    public class ConsoleLib
    {
        public static void RegisterModule(LuaTable mod)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            mod.SetNameValue("console", module);
        }
        
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("write", Write);
            module.Register("writeline", WriteLine);
            module.Register("clear", new LuaFunc(delegate (LuaValue[] args) { 
                                                     Console.Clear(); 
                                                     return LuaNil.Nil; 
                                                 }));
            module.Register("writecolor", WriteColor);
        }
        
        public static LuaValue Write(LuaValue[] args)
        {
            Console.Write(string.Join<LuaValue>("    ", args));
            return LuaNil.Nil;
        }
        
        public static LuaValue WriteLine(LuaValue[] args)
        {
            Console.WriteLine(string.Join<LuaValue>("    ", args));
            return LuaNil.Nil;
        }
        
        public static LuaValue WriteColor(LuaValue[] args)
        {
            // get and check color text
            string color = (args[0] as LuaString).Text;
            if (!(color.Substring(0, 1) == color.Substring(0, 1).ToUpper()))
                color = color.Substring(0, 1).ToUpper() + color.Substring(1);
            // that makes it a Capital Color not a lowercase color
            
            // get and set color
            ConsoleColor c = (ConsoleColor) Enum.Parse(typeof(ConsoleColor), color);
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = c;
            
            List<LuaValue> args2 = new List<LuaValue>();
            foreach (LuaValue v in args)
                args2.Add(v); // copy to new array
            args2.RemoveAt(0); // remove color
            Console.WriteLine(string.Join<LuaValue>("    ", args2.ToArray()));
            
            // reset color
            Console.ForegroundColor = old;
            return LuaNil.Nil;
        }
    }
}
