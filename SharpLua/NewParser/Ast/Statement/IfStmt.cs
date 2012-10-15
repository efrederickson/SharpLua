using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public class IfStmt : Chunk
    {
        public List<SubIfStmt> Clauses = new List<SubIfStmt>();
    }

    public abstract class SubIfStmt : Chunk
    {
        //public List<Statement> Body = null;
    }

    public class ElseIfStmt : SubIfStmt
    {
        public Expression.Expression Condition = null;
    }

    public class ElseStmt : SubIfStmt
    {
    }
}
