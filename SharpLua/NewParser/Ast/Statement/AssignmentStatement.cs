using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class AssignmentStatement : Statement
    {
        public List<Expression.Expression> Lhs = new List<Expression.Expression>();
        public List<Expression.Expression> Rhs = new List<Expression.Expression>();
        public bool IsLocal = false;

        public override Statement Simplify()
        {
            for (int i = 0; i < Lhs.Count; i++)
                Lhs[i] = Lhs[i].Simplify();
            for (int i = 0; i < Rhs.Count; i++)
                Rhs[i] = Rhs[i].Simplify();
            return base.Simplify();
        }
    }
}
