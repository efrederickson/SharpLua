using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class NumericForStatement : Chunk
    {
        public Variable Variable = null;
        public Expression.Expression Start = null;
        public Expression.Expression End = null;
        public Expression.Expression Step = null;

        public NumericForStatement()
        {
            Scope = new Scope();
        }
    }

    public class GenericForStatement : Chunk
    {
        public Variable Variable = null;
        public List<Variable> VariableList = null;
        public List<Expression.Expression> Generators = null;

        public GenericForStatement()
        {
            Scope = new Scope();
        }
    }
}
