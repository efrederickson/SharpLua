using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class AnonymousFunctionExpr : Expression
    {
        public List<Statement.Statement> Body = null;
        public bool IsLocal = false;
        public bool IsVararg = false;
        public List<Variable> Arguments = new List<Variable>();
    }
}
