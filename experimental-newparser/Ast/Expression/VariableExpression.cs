using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class VariableExpression
    {
        public string Name;
        public Variable Var;
        public bool IsGlobal = false;
    }
}
