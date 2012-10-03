using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class RepeatStmt : Statement
    {
        public Chunk Body;

        public Expr Condition;

    }
}
