using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class RepeatStatement : Chunk
    {
        public Expression.Expression Condition = null;

        public RepeatStatement(Scope s)
            : base(new Scope(s))
        {

        }
    }
}
