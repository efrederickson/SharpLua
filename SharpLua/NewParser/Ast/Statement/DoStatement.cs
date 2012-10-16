using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class DoStatement : Chunk
    {
        public DoStatement(Scope s)
            : base(new Scope(s))
        {

        }
    }
}
