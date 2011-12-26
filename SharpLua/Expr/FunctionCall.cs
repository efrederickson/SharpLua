using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public partial class FunctionCall : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaFunction function = baseValue as LuaFunction;

            if (function != null)
            {
                if (function.Function.Method.DeclaringType.FullName == "SharpLua.Library.BaseLib" &&
                    (function.Function.Method.Name == "loadstring" || function.Function.Method.Name == "dofile"))
                {
                    if (this.Args.String != null)
                    {
                        return function.Function.Invoke(new LuaValue[] { this.Args.String.Evaluate(enviroment), enviroment });
                    }
                    else
                    {
                        return function.Function.Invoke(new LuaValue[] { this.Args.ArgList[0].Evaluate(enviroment), enviroment });
                    }
                }

                if (this.Args.Table != null)
                {
                    return function.Function.Invoke(new LuaValue[] { this.Args.Table.Evaluate(enviroment) });
                }
                else if (this.Args.String != null)
                {
                    return function.Function.Invoke(new LuaValue[] { this.Args.String.Evaluate(enviroment) });
                }
                else
                {
                    List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                    return function.Function.Invoke(LuaMultiValue.UnWrapLuaValues(args.ToArray()));
                }
            }
            else if ((baseValue as LuaTable) != null)
            {
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                args.Insert(0, baseValue);
                return ((baseValue as LuaTable).MetaTable.GetValue("__call") as LuaFunction).Invoke(args.ToArray());
            }
            else if ((baseValue as LuaClass) != null)
            {
                LuaClass c = baseValue as LuaClass;
                List<LuaValue> args = this.Args.ArgList.ConvertAll(arg => arg.Evaluate(enviroment));
                //args.Insert(0, new LuaString(this.Method);
                if (c.Self.MetaTable == null)
                    c.GenerateMetaTable();
                return (c.Self.MetaTable.GetValue("__call") as LuaFunction).Invoke(args.ToArray());
            }
            else
            {
                throw new Exception("Invoke function call on non function value.");
            }
        }
    }
}
