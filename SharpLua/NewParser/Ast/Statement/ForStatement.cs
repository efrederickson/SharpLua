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

        public override Statement Simplify()
        {
            Start = Start.Simplify();
            End = End.Simplify();
            Step = Step.Simplify();
            return base.Simplify();
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

        public override Statement Simplify()
        {
            for (int i = 0; i < Generators.Count; i++)
                Generators[i] = Generators[i].Simplify();
            return base.Simplify();
        }
    }
}
