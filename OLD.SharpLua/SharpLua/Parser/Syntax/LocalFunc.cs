using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class LocalFunc : Statement
    {
        public string Name;

        public FunctionBody Body;

    }
}
