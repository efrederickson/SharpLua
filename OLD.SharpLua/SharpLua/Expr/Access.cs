using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public abstract partial class Access
    {
        public abstract LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment);
    }
}
