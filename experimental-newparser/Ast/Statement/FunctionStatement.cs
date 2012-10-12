using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class FunctionStatement : Chunk
    {
        public bool IsLocal = false;
        public bool IsVararg = false;
        public List<Variable> Arguments = new List<Variable>();
        public Expression.Expression Name = null;

        public FunctionStatement()
        {
            Scope = new Scope();
        }
    }
}
