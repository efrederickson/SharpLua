using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class TableConstructorExpr : Expression
    {
        public List<Expression> EntryList = new List<Expression>();
    }
}
