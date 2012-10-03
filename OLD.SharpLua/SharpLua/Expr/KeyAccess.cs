using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class KeyAccess : Access
    {
        public override LuaValue Evaluate(LuaValue baseValue, LuaTable enviroment)
        {
            LuaValue key = this.Key.Evaluate(enviroment);
            return LuaValue.GetKeyValue(baseValue, key);
        }
    }
}
