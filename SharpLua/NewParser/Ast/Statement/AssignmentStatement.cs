using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class AssignmentStatement : Statement
    {
        public List<Expression.Expression> Lhs = null;
        public List<Expression.Expression> Rhs = null;
        public bool IsLocal = false;
    }
}
