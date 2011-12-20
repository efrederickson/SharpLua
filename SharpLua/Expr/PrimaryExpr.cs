using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class PrimaryExpr : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            LuaValue baseValue = this.Base.Evaluate(enviroment);

            foreach (Access access in this.Accesses)
            {
                baseValue = access.Evaluate(baseValue, enviroment);
            }

            return baseValue;
        }

        public override Term Simplify()
        {
            if (this.Accesses.Count == 0)
            {
                return this.Base.Simplify();
            }
            else
            {
                return this;
            }
        }
    }
}
