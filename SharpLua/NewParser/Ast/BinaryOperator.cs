using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast
{
    public enum BinaryOperator
    {
        Add, // +
        Subtract, // -
        Multiply, // *
        Divide, // /
        Power, // ^
        Modulus, // %
        Concat, // ..

        And, // and
        Or, // or
        LessThan, // <
        LessThanOrEqualTo, // <=
        GreaterThan, // >
        GreaterThanOrEqualTo, // >=
        NotEqual, // ~=
        Equals, // ==

        ShiftRight, // >>
        ShiftLeft, // <<
        Xor, // **
        BitAnd, // &
        BitOr, // |
        BitNot, // ~

        NONE = -1,
    }

    public enum UnaryOperator
    {
        Not, // !, not
        Length, // #
        BitNot, // ~
        Negate, // -
        UnNegate, // +

        NONE = -1,
    }
}
