using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class IfStmt : Statement
    {
        public Expr Condition;

        public Chunk ThenBlock;

        public List<ElseifBlock> ElseifBlocks = new List<ElseifBlock>();

        public Chunk ElseBlock;

    }
}
