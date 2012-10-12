using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class MemberExpr : Expression
    {
        public Expression Base = null;
        public string Indexer = ""; // either '.' or ':'
        public string Ident = "";
    }
}
