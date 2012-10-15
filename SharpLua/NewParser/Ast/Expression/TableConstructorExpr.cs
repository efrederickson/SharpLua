using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class TableConstructorExpr : Expression
    {
        public List<Expression> EntryList = new List<Expression>();
    }
}
