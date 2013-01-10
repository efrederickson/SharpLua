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
            else if (Op == "==")
                return BinaryOperator.Equals;

            return BinaryOperator.NONE;
        }

        public override Expression Simplify()
        {
            Lhs = Lhs.Simplify();
            Rhs = Rhs.Simplify();

            double a, b;
            int isNumA = Lua.luaO_str2d(((NumberExpr)Lhs).Value, out a);
            int isNumB = Lua.luaO_str2d(((NumberExpr)Rhs).Value, out b);
            bool isNum = false;
            isNum = isNumA == 1 && isNumB == 1;

            switch (GetOperator())
            {
                case BinaryOperator.Add:
                    if (isNum)
                        return new NumberExpr(a + b);
                    break;
                case BinaryOperator.Subtract:
                    if (isNum)
                        return new NumberExpr(a - b);
                    break;
                case BinaryOperator.Multiply:
                    if (isNum)
                        return new NumberExpr(a * b);
                    break;
                case BinaryOperator.Divide:
                    if (isNum)
                        return new NumberExpr(a / b);
                    break;
                case BinaryOperator.Power:
                    if (isNum)
                        return new NumberExpr(Math.Pow(a, b));
                    break;
                case BinaryOperator.Modulus:
                    if (isNum)
                        return new NumberExpr(a % b);
                    break;
                case BinaryOperator.LessThan:
                    if (isNum)
                        return new BoolExpr(a < b);
                    break;
                case BinaryOperator.LessThanOrEqualTo:
                    if (isNum)
                        return new BoolExpr(a <= b);
                    break;
                case BinaryOperator.GreaterThan:
                    if (isNum)
                        return new BoolExpr(a > b);
                    break;
                case BinaryOperator.GreaterThanOrEqualTo:
                    if (isNum)
                        return new BoolExpr(a >= b);
                    break;
                case BinaryOperator.NotEqual:
                    if (Lhs is BoolExpr && Rhs is BoolExpr)
                        return new BoolExpr(((BoolExpr)Lhs).Value != ((BoolExpr)Rhs).Value);
                    else if (isNum)
                        return new BoolExpr(a != b);
                    break;
                case BinaryOperator.Equals:
                    if (Lhs is BoolExpr && Rhs is BoolExpr)
                        return new BoolExpr(((BoolExpr)Lhs).Value == ((BoolExpr)Rhs).Value);
                    else if (isNum)
                        return new BoolExpr(a == b);
                    break;
                case BinaryOperator.And:
                    if (Lhs is BoolExpr && Rhs is BoolExpr)
                        return new BoolExpr(((BoolExpr)Lhs).Value && ((BoolExpr)Rhs).Value);
                    break;
                case BinaryOperator.Or:
                    if (Lhs is BoolExpr && Rhs is BoolExpr)
                        return new BoolExpr(((BoolExpr)Lhs).Value || ((BoolExpr)Rhs).Value);
                    break;
                case BinaryOperator.ShiftRight:
                case BinaryOperator.ShiftLeft:
                case BinaryOperator.Xor:
                case BinaryOperator.BitAnd:
                case BinaryOperator.BitOr:
                case BinaryOperator.BitNot:
                case BinaryOperator.Concat:
                case BinaryOperator.NONE:
                default:
                    break;
            }

            return base.Simplify();
        }
    }
}
