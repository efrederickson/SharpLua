using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class UsingStatement : Chunk
    {
        public AssignmentStatement Vars = null;
    }
}
