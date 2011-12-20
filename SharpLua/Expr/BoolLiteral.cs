using System;
using System.Collections.Generic;
using System.Text;

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
