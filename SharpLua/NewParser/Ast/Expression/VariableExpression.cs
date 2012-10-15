using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public class VariableExpression : Expression
    {
        public string Name;
        public Variable Var;
        public bool IsGlobal = false;
    }
}
