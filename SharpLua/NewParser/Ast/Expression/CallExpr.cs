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

        public override Expression Simplify()
        {
            Base = Base.Simplify();
            for (int i = 0; i < Arguments.Count; i++)
                Arguments[i] = Arguments[i].Simplify();
            return this;
        }
    }
}
