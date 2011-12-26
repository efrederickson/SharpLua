using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class ReturnStmt : Statement
    {
        public List<Expr> ExprList = new List<Expr>();

    }
}
