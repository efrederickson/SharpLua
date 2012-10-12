using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public class MatchWithStatement : Chunk
    {
        public List<Expression.Expression> Exprs = new List<Expression.Expression>();
        public List<MatchClauseStatement> Clauses = new List<MatchClauseStatement>();

        public MatchWithStatement()
        {
            Scope = new Scope();
        }
    }

    public class MatchClauseStatement : Chunk
    {
        public List<Expression.Expression> Exprs = new List<Expression.Expression>();
    }
}
