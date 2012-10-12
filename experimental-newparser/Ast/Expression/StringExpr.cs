using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class StringExpr : Expression
    {
        public string Value;
        public string Delimiter;


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
