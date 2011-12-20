using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class LocalVar : Statement
    {
        public List<string> NameList = new List<string>();

        public List<Expr> ExprList = new List<Expr>();

    }
}
