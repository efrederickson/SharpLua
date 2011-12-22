using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class ReturnStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            throw new NotImplementedException();
        }
    }
}
