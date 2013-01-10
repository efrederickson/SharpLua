using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class NumberExpr : Expression
    {
        public string Value;

        public NumberExpr() { }
        public NumberExpr(string value) { Value = value; }
        public NumberExpr(double value) { Value = value.ToString(); }
    }
}
