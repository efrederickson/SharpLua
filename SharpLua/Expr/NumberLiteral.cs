using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class NumberLiteral : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            double number;

            if (string.IsNullOrEmpty(this.HexicalText))
            {
                number = double.Parse(this.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent);
            }
            else
            {
                number = int.Parse(this.HexicalText, NumberStyles.HexNumber);
            }

            return new LuaNumber (number);
        }
    }
}
