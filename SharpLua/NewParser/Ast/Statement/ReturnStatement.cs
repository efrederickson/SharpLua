using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class ReturnStatement : Statement
    {
        public List<Expression.Expression> Arguments = null;
    }
}
