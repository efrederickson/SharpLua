using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class WhileStmt : Statement
    {
        public Expr Condition;

        public Chunk Body;

    }
}
