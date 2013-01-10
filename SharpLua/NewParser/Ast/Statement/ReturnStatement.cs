using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class ReturnStatement : Statement
    {
        public List<Expression.Expression> Arguments = null;

        public override Statement Simplify()
        {
            for (int i = 0; i < Arguments.Count; i++)
                Arguments[i] = Arguments[i].Simplify();
            return base.Simplify();
        }
    }
}
