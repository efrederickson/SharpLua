using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class CallStatement : Statement
    {
        // Is a CallExpr
        public Expression.Expression Expression = null;
    }
}
