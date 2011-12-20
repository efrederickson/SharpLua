using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
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
            else
            {
                throw new Exception("Invoke function call on non function value.");
            }
        }
    }
}
