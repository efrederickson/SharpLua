using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public abstract class Expression
    {
        public int ParenCount = 0;
        public Scope Scope = null;

        public virtual Expression Simplify()
        {
            return this;
        }
    }
}
