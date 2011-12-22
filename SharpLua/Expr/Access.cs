using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public abstract partial class Access
    {
        public abstract LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment);
    }
}
