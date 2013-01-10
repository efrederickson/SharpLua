using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class IfStmt : Chunk
    {
        public List<SubIfStmt> Clauses = new List<SubIfStmt>();

        public override Statement Simplify()
        {
            for (int i = 0; i < Clauses.Count; i++)
                Clauses[i].Simplify();
            return base.Simplify();
        }
    }

    public abstract class SubIfStmt : Chunk
    {
        public SubIfStmt(Scope s)
            : base(s)
        {

        }
    }

    public class ElseIfStmt : SubIfStmt
    {
        public Expression.Expression Condition = null;

        public ElseIfStmt(Scope s)
            : base(new Scope(s))
        {

        }

        public override Statement Simplify()
        {
            Condition = Condition.Simplify();
            return base.Simplify();
        }
    }

    public class ElseStmt : SubIfStmt
    {
        public ElseStmt(Scope s)
            : base(new Scope(s))
        {

        }
    }
}
