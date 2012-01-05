using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using SharpLua.Library;
using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class MethodCall : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaValue value = null;
		try {LuaValue.GetKeyValue(baseValue, new LuaString(this.Method)); } catch (Exception) { }
            LuaFunction function = value as LuaFunction;

            if (function != null)
            {
                if (this.Args.Table != null)
                {
                    return function.Function.Invoke(new LuaValue[] { baseValue, this.Args.Table.Evaluate(enviroment) });
                }
                else if (this.Args.String != null)
                {
                    return function.Function.Invoke(new LuaValue[] { baseValue, this.Args.String.Evaluate(enviroment) });
                }
                else
                {
                    List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                    args.Insert(0, baseValue);
                    return function.Function.Invoke(args.ToArray());
                }
            } // method call on table would be like _G:script()
            else if ((baseValue as LuaTable) != null)
            {
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                return ((baseValue as LuaTable).MetaTable.GetValue("__call") as LuaFunction).Invoke(args.ToArray());
            }
            else if ((baseValue as LuaClass) != null)
            {
                LuaClass c = baseValue as LuaClass;
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                args.Insert(0, new LuaString(this.Method));
                if (c.Self.MetaTable == null)
                    c.GenerateMetaTable();
                return (c.Self.MetaTable.GetValue("__call") as LuaFunction).Invoke(args.ToArray());
            }
            else if ((baseValue as LuaUserdata) != null)
            {
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                LuaUserdata obj = baseValue as LuaUserdata;
                object o = obj.Value;
                if (obj.MetaTable != null)
                {
                    if (obj.MetaTable.GetValue(this.Method) != null)
                    {
                        LuaValue o2 = obj.MetaTable.GetValue(this.Method);
                        if ((o2 as LuaFunction) != null)
                            return (o2 as LuaFunction).Invoke(args.ToArray());
                        else if ((o2 as LuaTable) != null)
                            throw new NotImplementedException(); // TODO
                    }
                }
                return ScriptLib.ToLuaValue(o.GetType().GetMethod(this.Method, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(o, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args.ToArray(), CultureInfo.CurrentCulture));
            }
            else
            {
                throw new Exception("Invoke method call on non function value.");
            }
        }
    }
}
