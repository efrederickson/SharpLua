using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class FunctionStatement : Chunk
    {
        public bool IsLocal = false;
        public bool IsVararg = false;
        public List<Variable> Arguments = new List<Variable>();
        public Expression.Expression Name = null;

        public FunctionStatement(Scope s)
            : base(s)
        {

        }

        public override Statement Simplify()
        {
            Name = Name.Simplify();
            return base.Simplify();
        }
    }
}
