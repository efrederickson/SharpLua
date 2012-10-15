using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class UnOpExpr : Expression
    {
        public Expression Rhs = null;
        public string Op = "";
    }
}
