using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class MethodCall : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaValue value = LuaValue.GetKeyValue(baseValue, new LuaString(this.Method));
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
            }
            else if ((value as LuaTable) != null)
            {
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                return ((value as LuaTable).MetaTable.GetValue("__call") as LuaFunction).Invoke(args.ToArray());
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
            else
            {
                throw new Exception("Invoke method call on non function value.");
            }
        }
    }
}
