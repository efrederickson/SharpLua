/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/22/2011
 * Time: 3:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// coroutine library
    /// </summary>
    public class CoroutineLib
    {
        
        public static void RegisterModule(LuaTable env)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            env.SetNameValue("coroutine", module);
        }
        
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("create", Create);
            module.Register("resume", Resume);
            module.Register("running", Running);
            module.Register("status", Status);
            module.Register("wrap", Wrap);
            module.Register("yield", Yield);
        }
        
        public static LuaValue Create(LuaValue[] args)
        {
            LuaFunction func = args[0] as LuaFunction;
            if (func == null)
                throw new ArgumentException("Function expected, got '" + args[0].Value.GetType().Name + "'");
            LuaCoroutine c = new LuaCoroutine(func);
            return c;
        }
        
        public static LuaValue Resume(LuaValue[] args)
        {
            LuaCoroutine c = args[0] as LuaCoroutine;
            List<LuaValue> args2 = new List<LuaValue>();
            foreach (LuaValue v in args)
                args2.Add(v);
            args2.RemoveAt(0); // remove coroutine
            
            c.Resume(args2.ToArray());
            return LuaNil.Nil;
        }
        
        public static LuaValue Running(LuaValue[] args)
        {
            if (LuaCoroutine.Running == null)
                return LuaNil.Nil;
            return LuaCoroutine.Running;
        }
        
        public static LuaValue Status(LuaValue[] args)
        {
            LuaCoroutine c = args[0] as LuaCoroutine;
            return new LuaString(c.Status);
        }
        
        public static LuaValue Wrap(LuaValue[] args)
        {
            LuaFunction f = args[0] as LuaFunction;
            LuaCoroutine c = new LuaCoroutine(f);
            LuaFunction f2 = new LuaFunction(new LuaFunc(delegate(LuaValue[] args2) { return LuaBoolean.From(c.Resume(args2)); }));
            return f2;
        }
        
        public static LuaValue Yield(LuaValue[] args)
        {
            LuaCoroutine c = args[0] as LuaCoroutine;
            c.Pause();
            // TODO: set restart args
            return LuaNil.Nil;
        }
    }
}
