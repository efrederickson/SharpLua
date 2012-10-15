using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class BinOpExpr : Expression
    {
        public Expression Lhs = null;
        public Expression Rhs = null;
        public string Op = "";
    }
}
