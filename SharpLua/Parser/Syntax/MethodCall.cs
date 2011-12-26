using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class MethodCall : Access
    {
        public string Method;

        public Args Args;

    }
}
