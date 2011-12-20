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
using System.Reflection;

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
            module.Register("call", Call);
            module.Register("reference", Reference);
            module.Register("create", Create);
            module.Register("import", Import);
        }
        
        /// <summary>
        /// calls a .NET method on object
        /// format: script.call(object, function, ...) where ... is any args
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static LuaValue Call(LuaValue[] args)
        {
            string method = args[1].Value.ToString();
            List<object> _args = new List<object>();
            foreach (LuaValue a in args)
                _args.Add(a.Value);
            _args.RemoveRange(0, 2);
            
            BindingFlags bindingFlags = BindingFlags.IgnoreCase
                | BindingFlags.Public
                | BindingFlags.NonPublic;

            // Is it a method on a static type or an object instance ?
            Type type;
            bindingFlags = bindingFlags | BindingFlags.Instance;
            type = args[0].Value.GetType();

            // Start by looking for a method call
            MethodInfo m = type.GetMethod(method.ToString(),
                                          bindingFlags | BindingFlags.InvokeMethod);
            if (m != null)
                return new LuaUserdata(m.Invoke(args[0].Value, _args.ToArray()));

            // Now loook for a property get
            PropertyInfo p = type.GetProperty(method.ToString(), bindingFlags | BindingFlags.GetProperty);
            if (p != null)
                return new LuaUserdata(p.GetGetMethod().Invoke(args[0].Value,_args.ToArray()));

            // Now look for a field get
            FieldInfo f = type.GetField(method.ToString(),bindingFlags | BindingFlags.GetField);
            if (f != null)
                return new LuaUserdata(f.GetValue(args[0].Value));
            

            // FIXME: or an event ?
            EventInfo e  = type.GetEvent(method.ToString(), bindingFlags); // | BindingFlags.Event)
            if (e != null) // attempt to call the click event
                return new LuaUserdata(e.GetRaiseMethod().Invoke(args[0].Value, _args.ToArray()));

            throw new LuaError("Cannot find method '" + args[0].Value.ToString() + "' on object of type '" + args[1].Value.GetType().ToString() + "'!");
        }
        
        public static LuaValue Reference(LuaValue[] args)
        {
            foreach (LuaValue obj in args)
            {
                try {
                    Library.BaseLib.print(new LuaValue[] {new LuaString(AssemblyCache.Instance.LoadAssembly(obj.Value.ToString()).ToString())});
                } catch (Exception ex) {
                    Library.BaseLib.print(new LuaValue[] {new LuaString("Error adding reference to '" + obj.Value.ToString() + "'\n" + ex.Message)});
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
                BaseLib.print(new LuaValue[] { new LuaString("Cannot find type '" + args[0].Value.ToString() + "' in loaded assemblies!")});
            
            List<object> _args = new List<object>();
            int i = 0;
            foreach (LuaValue v in args)
            {
                if (i != 0)
                    _args.Add(v.Value);
                
                i++;
            }
            
            // create object.
            return new LuaUserdata(Activator.CreateInstance(t, _args.ToArray()));
        }
        
        public static LuaValue Import(LuaValue[] args)
        {
            foreach (LuaValue obj in args)
            {
                AssemblyCache.ImportNamespace(obj.Value.ToString());
            }
            return LuaNil.Nil;
        }
    }
}
