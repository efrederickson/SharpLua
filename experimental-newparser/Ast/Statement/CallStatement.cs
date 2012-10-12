using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class CallStatement : Statement
    {
        // Is a CallExpr
        public Expression.Expression Expression = null;
    }
}
