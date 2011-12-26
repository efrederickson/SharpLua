using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class ElseifBlock
    {
        public Expr Condition;

        public Chunk ThenBlock;

    }
}
