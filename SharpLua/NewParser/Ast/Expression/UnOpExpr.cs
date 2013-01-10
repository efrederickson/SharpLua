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

        public override Expression Simplify()
        {
            Rhs = Rhs.Simplify();
            UnaryOperator unop = GetOperator();
            if (Rhs is NumberExpr)
            {
                if (unop == UnaryOperator.Negate)
                    return new NumberExpr("-" + ((NumberExpr)Rhs).Value);
                else if (unop == UnaryOperator.UnNegate)
                {
                    double res;
                    if (Lua.luaO_str2d(((NumberExpr)Rhs).Value, out res) == 1)
                    {
                        return new NumberExpr(Math.Abs(res));
                    }
                }
            }
            else if (Rhs is BoolExpr)
                if (unop == UnaryOperator.Not)
                    return new BoolExpr(!((BoolExpr)Rhs).Value);

            return this;
        }
    }
}
