/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/22/2011
 * Time: 10:20 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// A garbage collector library
    /// </summary>
    public class GarbageCollectorLib
    {
        private static Thread thread = new Thread(new ThreadStart(_collectGarbage));
        public GarbageCollectorLib()
        {
        }
        
        public static void RegisterModule(LuaTable module)
        {
            module.Register("collectgarbage", CollectGarbage);
        }
        
        public static LuaValue CollectGarbage(LuaValue[] args)
        {
            string job = (args[0] as LuaString).Text.ToLower();
            if (job == "collect")
            {
                // scan for nil items and remove them.
                thread.Start();
            }
            else if (job == "stop")
            {
                // stop collector
                try {
                    thread.Suspend();
                } catch (ThreadStateException ) {
                    throw new Exception("Cannot stop garbage collector, it is not running!");
                } catch (Exception ex) {
                    throw ex;
                }
                
            }
            else if (job == "info")
            {
                // return info from last/current collection
            }
            return LuaNil.Nil;
        }
        
        private static void _collectGarbage()
        {
            DateTime now = DateTime.Now;
            int result = ScanTable(Lua.GlobalEnvironment);
            DateTime newNow = DateTime.Now;
            Console.WriteLine("Garbage Collector: removed '" + result + "' dead items. Time taken: " + (now - newNow).ToString());
        }
        
        private static int ScanTable(LuaTable t)
        {
            int r = 0;
            LuaTable t2 = t;
            // scan keys
            foreach (LuaValue key in t.Keys)
            {
                if ((key == LuaNil.Nil) || (key == null))
                {
                    // its dead, remove it.
                    t2.RemoveKey(key);
                    r++;
                }
            }
            // scan child tables + child tables
            foreach (LuaValue val in t.ListValues)
            {
                if ((val == LuaNil.Nil) || val == null)
                {
                    t2.Remove(val);
                    r++;
                }
                
                if (val is LuaTable)
                    r += ScanTable(val as LuaTable);
            }
            t = t2;
            
            return r;
        }
    }
}
