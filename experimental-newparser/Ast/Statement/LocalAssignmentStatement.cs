using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class LocalAssignmentStatement : Statement
    {
        public List<Variable> LocalList = null;
        public List<Expression.Expression> InitList = null;
    }
}
