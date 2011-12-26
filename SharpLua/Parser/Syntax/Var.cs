using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class Var
    {
        public BaseExpr Base;

        public List<Access> Accesses = new List<Access>();

    }
}
