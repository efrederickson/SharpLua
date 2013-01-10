using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class WhileStatement : Chunk
    {
        public Expression.Expression Condition = null;

        public WhileStatement(Scope s)
            : base(new Scope(s))
        {

        }

        public override Statement Simplify()
        {
            Condition = Condition.Simplify();
            return base.Simplify();
        }
    }
}
