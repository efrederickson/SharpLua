/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/20/2011
 * Time: 10:34 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public class ScriptLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("script", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("reference", Reference);
            module.Register("create", Create);
            module.Register("import", Import);
            module.Register("getinfo", GetInfo);
            module.Register("dump", Dump);
            module.Register("call", Call);
            LuaTable mt = new LuaTable();
            mt.Register("__index", new LuaFunc((LuaValue[] args) =>
                                               {
                                                   return Create(args);
                                               }));
            mt.Register("__call", new LuaFunc((LuaValue[] args) =>
                                              {
                                                  return Create(args);
                                              }));
            module.MetaTable = mt;
        }
        
        public static LuaValue Reference(LuaValue[] args)
        {
            foreach (LuaValue obj in args)
            {
                try {
                    Library.BaseLib.Print(new LuaValue[] {new LuaString(AssemblyCache.Instance.LoadAssembly(obj.Value.ToString()).ToString())});
                } catch (Exception ex) {
                    Library.BaseLib.Print(new LuaValue[] {new LuaString("Error adding reference to '" + obj.Value.ToString() + "'\n" + ex.Message)});
                }
            }
            return LuaNil.Nil;
        }
        
        /// <summary>
        /// Creates a .NET object
        /// format: script.create(type, ...) where ... is constructor args
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static LuaValue Create(LuaValue[] args)
        {
            Type t = null;
            
            t = AssemblyCache.FindType(args[0].Value.ToString());
            
            if (t == null)
                BaseLib.Print(new LuaValue[] { new LuaString("Cannot find type '" + args[0].Value.ToString() + "' in loaded assemblies!")});
            
            List<object> _args = new List<object>();
            int i = 0;
            foreach (LuaValue v in args)
            {
                if (i != 0)
                    _args.Add(v.Value);
                
                i++;
            }
            
            // create object.
            LuaUserdata obj = new LuaUserdata(Activator.CreateInstance(t, _args.ToArray()));
            return ObjectToLua.ToLuaValue(obj);
        }
        
        public static LuaValue Import(LuaValue[] args)
        {
            foreach (LuaValue obj in args)
            {
                AssemblyCache.ImportNamespace(obj.Value.ToString());
            }
            return LuaNil.Nil;
        }
        
        public static LuaValue GetInfo(LuaValue[] args)
        {
            return new LuaString(Inspector.Inspect(args[0].Value));
        }
        
        public static LuaValue Dump(LuaValue[] args)
        {
            Console.WriteLine(Inspector.Inspect(args[0].Value));
            return LuaNil.Nil;
        }
        
        public static LuaValue Call(LuaValue[] args)
        {
            string method = args[0].ToString();
            List<object> args2 = new List<object>();
            for (int i = 1; i < args.Length; i++)
                args2.Add(args[i].Value);
            string n, m;
            n = method.Substring(0, method.LastIndexOf("."));
            m = method.Substring(method.LastIndexOf("."));
            AssemblyCache.ImportNamespace(n);
            Type t = AssemblyCache.FindType(n);
            if (t == null)
                throw new Exception("Cannot find type '" + n + "'!");
            BindingFlags bindingFlags = BindingFlags.IgnoreCase
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.FlattenHierarchy;
            
            // Start by looking for a method call
            MethodInfo m2 = t.GetMethod(m,
                                        bindingFlags | BindingFlags.InvokeMethod
                                       );
            if (m2 != null)
                return ObjectToLua.ToLuaValue(m2.Invoke(t, args2.ToArray()));

            // Now loook for a property get
            PropertyInfo p = t.GetProperty(m, bindingFlags | BindingFlags.GetProperty);
            if (p != null)
                return ObjectToLua.ToLuaValue(p.GetGetMethod().Invoke(t, args2.ToArray()));

            // Now look for a field get
            FieldInfo f = t.GetField(m, bindingFlags | BindingFlags.GetField);
            if (f != null)
                return ObjectToLua.ToLuaValue(f.GetValue(t));
            
            throw new Exception("Cannot find method '" + m + "' on class '" + n + "'!");
        }
    }
}
