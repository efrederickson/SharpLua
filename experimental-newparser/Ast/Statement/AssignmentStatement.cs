using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class AssignmentStatement : Statement
    {
        public List<Expression.Expression> Lhs = null;
        public List<Expression.Expression> Rhs = null;
    }
}
