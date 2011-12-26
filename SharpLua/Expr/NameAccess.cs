using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public partial class NameAccess : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaValue key = new LuaString(this.Name);
            return LuaValue.GetKeyValue(baseValue, key);
        }
    }
}
