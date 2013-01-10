using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class MemberExpr : Expression
    {
        public Expression Base = null;
        public string Indexer = ""; // either '.' or ':'
        public string Ident = "";

        public override Expression Simplify()
        {
            Base = Base.Simplify();
            return this;
        }
    }
}
