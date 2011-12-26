using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class Function : Statement
    {
        public FunctionName Name;

        public FunctionBody Body;

    }
}
