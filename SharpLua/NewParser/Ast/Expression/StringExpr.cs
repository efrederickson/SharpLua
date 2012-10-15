using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class StringExpr : Expression
    {
        public string Value;
        public TokenType StringType;


        public StringExpr()
        {
            Value = "";
        }

        public StringExpr(string v)
        {
            Value = v;
        }
    }
}
