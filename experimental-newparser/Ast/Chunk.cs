using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast
{
    public class Chunk : Statement.Statement
    {
        public List<Statement.Statement> Body = new List<Statement.Statement>();

        public Chunk()
        {

        }

        public Chunk(Scope s)
        {
            Scope = s;
        }
    }
}
