using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// Represent Unary or Binary Operation, for Unary Operation the LeftOperand is not used.
    /// </summary>
    public partial class Operation : Term
    {
        public string Operator;

        public Term LeftOperand;

        public Term RightOperand;

        public Operation(string oper)
        {
            this.Operator = oper;
        }

        public Operation(string oper, Term left, Term right)
        {
            this.Operator = oper;
            this.LeftOperand = left == null ? null : left.Simplify();
            this.RightOperand = right == null ? null : right.Simplify();
        }

        public override LuaValue Evaluate(LuaTable enviroment)
        {
            if (this.LeftOperand == null)
            {
                return PrefixUnaryOperation(Operator, RightOperand, enviroment);
            }
            else if (this.RightOperand == null)
            {
                return LeftOperand.Evaluate(enviroment);
            }
            else
            {
                return InfixBinaryOperation(LeftOperand, Operator, RightOperand, enviroment);
            }
        }

        private LuaValue PrefixUnaryOperation(string Operator, Term RightOperand, LuaTable enviroment)
        {
            LuaValue rightValue = RightOperand.Evaluate(enviroment);

            switch (Operator)
            {
                case "-":
                    var number = rightValue as LuaNumber;
                    if (number != null)
                    {
                        return new LuaNumber(-number.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__unm", rightValue, null);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { rightValue });
                        }
                    }
                    break;
                case "#":
                    var table = rightValue as LuaTable;
                    if (table != null)
                    {
                        return new LuaNumber(table.Length);
                    }
                    var str = rightValue as LuaString;
                    if (str != null)
                    {
                        return new LuaNumber(str.Text.Length);
                    }
                    break;
                case "not":
                    var rightBool = rightValue as LuaBoolean;
                    if (rightBool != null)
                    {
                        return LuaBoolean.From(!rightBool.BoolValue);
                    }
                    break;
            }

            return LuaNil.Nil;
        }

        private LuaValue InfixBinaryOperation(Term LeftOperand, string Operator, Term RightOperand, LuaTable enviroment)
        {
            LuaValue leftValue = LeftOperand.Evaluate(enviroment);
            LuaValue rightValue = RightOperand.Evaluate(enviroment);

            switch (Operator)
            {
                case "+":
                    var left = leftValue as LuaNumber;
                    var right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(left.Number + right.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__add", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "-":
                    left = leftValue as LuaNumber;
                    right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(left.Number - right.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__sub", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "*":
                    left = leftValue as LuaNumber;
                    right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(left.Number * right.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__mul", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "/":
                    left = leftValue as LuaNumber;
                    right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(left.Number / right.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__div", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "%":
                    left = leftValue as LuaNumber;
                    right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(left.Number % right.Number);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__mod", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "^":
                    left = leftValue as LuaNumber;
                    right = rightValue as LuaNumber;
                    if (left != null && right != null)
                    {
                        return new LuaNumber(Math.Pow(left.Number, right.Number));
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__pow", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "==":
                    return LuaBoolean.From(leftValue.Equals(rightValue));
                case "~=":
                    return LuaBoolean.From(leftValue.Equals(rightValue) == false);
                case "<":
                    int? compare = Compare(leftValue, rightValue);
                    if (compare != null)
                    {
                        return LuaBoolean.From(compare < 0);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__lt", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case ">":
                    compare = Compare(leftValue, rightValue);
                    if (compare != null)
                    {
                        return LuaBoolean.From(compare > 0);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__gt", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "<=":
                    compare = Compare(leftValue, rightValue);
                    if (compare != null)
                    {
                        return LuaBoolean.From(compare <= 0);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__le", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case ">=":
                    compare = Compare(leftValue, rightValue);
                    if (compare != null)
                    {
                        return LuaBoolean.From(compare >= 0);
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__ge", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "..":
                    if ((leftValue is LuaString || leftValue is LuaNumber) &&
                        (rightValue is LuaString || rightValue is LuaNumber))
                    {
                        return new LuaString(string.Concat(leftValue, rightValue));
                    }
                    else
                    {
                        LuaFunction func = GetMetaFunction("__concat", leftValue, rightValue);
                        if (func != null)
                        {
                            return func.Invoke(new LuaValue[] { leftValue, rightValue });
                        }
                    }
                    break;
                case "and":
                    bool leftBool = leftValue.GetBooleanValue();
                    bool rightBool = rightValue.GetBooleanValue();
                    if (leftBool == false)
                    {
                        return leftValue;
                    }
                    else
                    {
                        return rightValue;
                    }
                case "or":
                    leftBool = leftValue.GetBooleanValue();
                    rightBool = rightValue.GetBooleanValue();
                    if (leftBool == true)
                    {
                        return leftValue;
                    }
                    else
                    {
                        return rightValue;
                    }
            }

            return null;
        }

        private static int? Compare(LuaValue leftValue, LuaValue rightValue)
        {
            LuaNumber left = leftValue as LuaNumber;
            LuaNumber right = rightValue as LuaNumber;
            if (left != null && right != null)
            {
                return left.Number.CompareTo(right.Number);
            }

            LuaString leftString = leftValue as LuaString;
            LuaString rightString = rightValue as LuaString;
            if (leftString != null && rightString != null)
            {
                return StringComparer.Ordinal.Compare(leftString.Text, rightString.Text);
            }

            return null;
        }

        private static LuaFunction GetMetaFunction(string name, LuaValue leftValue, LuaValue rightValue)
        {
            LuaTable left = leftValue as LuaTable;

            if (left != null)
            {
                LuaFunction func = left.GetValue(name) as LuaFunction;

                if (func != null)
                {
                    return func;
                }
            }

            LuaTable right = rightValue as LuaTable;

            if (right != null)
            {
                return right.GetValue(name) as LuaFunction;
            }

            return null;
        }
    }
}
