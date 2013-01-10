using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class TableConstructorStringKeyExpr : Expression
    {
        public string Key = "";
        public Expression Value = null;

        public override Expression Simplify()
        {
            Value = Value.Simplify();
            return this;
        }
    }
}
