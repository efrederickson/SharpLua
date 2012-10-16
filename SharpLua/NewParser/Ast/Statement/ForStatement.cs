using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class NumericForStatement : Chunk
    {
        public Variable Variable = null;
        public Expression.Expression Start = null;
        public Expression.Expression End = null;
        public Expression.Expression Step = null;

        public NumericForStatement(Scope s)
            : base(new Scope(s))
        {

        }
    }

    public class GenericForStatement : Chunk
    {
        public List<Variable> VariableList = null;
        public List<Expression.Expression> Generators = null;

        public GenericForStatement(Scope s)
            : base(new Scope(s))
        {

        }
    }
}
