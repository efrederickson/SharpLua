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

        public UnaryOperator GetOperator()
        {
            if (Op == "!" || Op == "not")
                return UnaryOperator.Not;
            else if (Op == "#")
                return UnaryOperator.Length;
            else if (Op == "~")
                return UnaryOperator.BitNot;
            else if (Op == "-")
                return UnaryOperator.Negate;
            else if (Op == "+")
                return UnaryOperator.UnNegate;
            else
                return UnaryOperator.NONE;
        }
    }
}
