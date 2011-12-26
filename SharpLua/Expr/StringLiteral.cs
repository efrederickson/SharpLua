using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public partial class StringLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return new LuaString(this.Text);
        }
    }
}
