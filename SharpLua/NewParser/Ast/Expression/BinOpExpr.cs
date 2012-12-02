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

        public BinaryOperator GetOperator()
        {
            if (Op == "+")
                return BinaryOperator.Add;
            else if (Op == "-")
                return BinaryOperator.Subtract;
            else if (Op == "*")
                return BinaryOperator.Multiply;
            else if (Op == "/")
                return BinaryOperator.Divide;
            else if (Op == "^")
                return BinaryOperator.Power;
            else if (Op == "%")
                return BinaryOperator.Modulus;
            else if (Op == "..")
                return BinaryOperator.Concat;
            else if (Op == "and")
                return BinaryOperator.And;
            else if (Op == "or")
                return BinaryOperator.Or;
            else if (Op == "<")
                return BinaryOperator.LessThan;
            else if (Op == "<=")
                return BinaryOperator.LessThanOrEqualTo;
            else if (Op == ">")
                return BinaryOperator.GreaterThan;
            else if (Op == ">=")
                return BinaryOperator.GreaterThanOrEqualTo;
            else if (Op == "~=" || Op == "!=")
                return BinaryOperator.NotEqual;
            else if (Op == ">>")
                return BinaryOperator.ShiftRight;
            else if (Op == "<<")
                return BinaryOperator.ShiftLeft;
            else if (Op == "^^")
                return BinaryOperator.Xor;
            else if (Op == "&")
                return BinaryOperator.BitAnd;
            else if (Op == "|")
                return BinaryOperator.BitOr;
            else if (Op == "~")
                return BinaryOperator.BitNot;

            return BinaryOperator.NONE;
        }
    }
}
