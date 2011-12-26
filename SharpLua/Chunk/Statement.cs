using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public abstract partial class Statement
    {
        public abstract LuaValue Execute(LuaTable enviroment, out bool isBreak);
    }
}
