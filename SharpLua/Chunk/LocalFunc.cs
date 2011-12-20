using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class LocalFunc : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            enviroment.SetNameValue(this.Name, this.Body.Evaluate(enviroment));
            isBreak = false;
            return null;
        }
    }
}
