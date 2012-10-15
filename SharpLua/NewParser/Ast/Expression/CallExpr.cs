using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class CallExpr : Expression
    {
        public Expression Base = null;
        public List<Expression> Arguments = new List<Expression>();
    }
}
