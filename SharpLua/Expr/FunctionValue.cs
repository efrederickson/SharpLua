using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public partial class FunctionValue : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return this.Body.Evaluate(enviroment);
        }
    }
}
