using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class InlineFunctionExpression : Expression
    {
        public List<Expression> Expressions = new List<Expression>();
        public bool IsVararg = false;
        public List<Variable> Arguments = new List<Variable>();
    }
}
