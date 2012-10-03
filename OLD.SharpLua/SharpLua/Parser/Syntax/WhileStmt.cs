using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class WhileStmt : Statement
    {
        /// <summary>
        /// The while Condition that is evaluated.
        /// </summary>
        public Expr Condition;
        
        /// <summary>
        /// The body of the code that is executed.
        /// </summary>
        public Chunk Body;
        
    }
}
