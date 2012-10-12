using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class RepeatStatement : Chunk
    {
        public Expression.Expression Condition = null;
    }
}
