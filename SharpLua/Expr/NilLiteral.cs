using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class NilLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return LuaNil.Nil;
        }
    }
}
