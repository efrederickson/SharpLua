using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class Assignment : Statement
    {
        public List<Var> VarList = new List<Var>();

        public List<Expr> ExprList = new List<Expr>();

    }
}
