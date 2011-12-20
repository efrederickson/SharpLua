using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class FunctionValue : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return this.Body.Evaluate(enviroment);
        }
    }
}
