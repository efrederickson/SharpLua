using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Expression
{
    public class TableConstructorKeyExpr : Expression
    {
        public Expression Key = null;
        public Expression Value = null;
    }
}
