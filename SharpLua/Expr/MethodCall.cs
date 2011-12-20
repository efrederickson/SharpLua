using System;
using System.Collections.Generic;
using System.Text;

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
            else
            {
                throw new Exception("Invoke method call on non function value.");
            }
        }
    }
}
