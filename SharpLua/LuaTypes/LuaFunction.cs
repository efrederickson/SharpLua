using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    public delegate LuaValue LuaFunc(LuaValue[] args);

    public class LuaFunction : LuaValue
    {
        public LuaFunction(LuaFunc function)
        {
            this.Function = function;
        }

        public LuaFunc Function { get; set; }

        public override object Value
        {
            get { return this.Function; }
        }

        public override string GetTypeCode()
        {
            return "function";
        }

        public LuaValue Invoke(LuaValue[] args)
        {
            return this.Function.Invoke(args);
        }
        
        public override string ToString()
        {
            if (this.MetaTable != null)
            {
                LuaFunction function = this.MetaTable.GetValue("__tostring") as LuaFunction;
                if (function != null)
                {
                    return function.Invoke(new LuaValue[] { this }).ToString();
                }
            }
            
            return "Function " + GetHashCode();
        }

    }
}
