using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast.Statement;

namespace SharpLua.Ast.Expression
{
    public class TableConstructorNamedFunctionExpr : Expression
    {
        public FunctionStatement Value;
    }
}
