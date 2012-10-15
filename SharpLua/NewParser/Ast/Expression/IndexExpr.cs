using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class IndexExpr : Expression
    {
        public Expression Base = null;
        public Expression Index = null;
    }
}
