using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class BoolLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return LuaBoolean.From(bool.Parse(this.Text));
        }
    }
}
