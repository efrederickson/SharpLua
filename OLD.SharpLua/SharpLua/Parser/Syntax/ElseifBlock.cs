using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class ElseifBlock
    {
        public Expr Condition;

        public Chunk ThenBlock;

    }
}
