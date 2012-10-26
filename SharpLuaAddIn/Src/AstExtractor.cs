using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua.Ast;

namespace SharpLuaAddIn
{
    class AstExtractor
    {
        private AstExtractor() { }

        public static List<CompletionItem> ExtractSymbols(Chunk c)
        {
            return DoChunk(c.Body);
        }

        static List<CompletionItem> DoExpr(Expression e)
        {
            List<CompletionItem> ret = new List<CompletionItem>();
            if (e is AnonymousFunctionExpr)
            {
                AnonymousFunctionExpr f = e as AnonymousFunctionExpr;
                for (int i = 0; i < f.Arguments.Count; i++)
                    ret.Add(new CompletionItem(f.Arguments[i].Name));
                ret.AddRange(DoChunk(f.Body));
            }
            else if (e is BinOpExpr)
            {
            }
            else if (e is BoolExpr)
            {
            }
            else if (e is CallExpr && (!(e is StringCallExpr) && !(e is TableCallExpr)))
            {
            }
            else if (e is StringCallExpr)
            {
                ret.AddRange(DoExpr(((StringCallExpr)e).Base));
            }
            else if (e is TableCallExpr)
            {
                ret.AddRange(DoExpr(((StringCallExpr)e).Base));
            }
            else if (e is IndexExpr)
            {
                ret.AddRange(DoExpr(((IndexExpr)e).Base));
                ret.AddRange(DoExpr(((IndexExpr)e).Index));
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
                InlineFunctionExpression ife = e as InlineFunctionExpression;
                for (int i = 0; i < ife.Arguments.Count; i++)
                    ret.Add(new CompletionItem(ife.Arguments[i].Name));
            }
            else if (e is TableConstructorKeyExpr)
            {
                TableConstructorKeyExpr t = e as TableConstructorKeyExpr;
                ret.AddRange(DoExpr(t.Key));
                ret.AddRange(DoExpr(t.Value));
            }
            else if (e is MemberExpr)
            {
                MemberExpr m = e as MemberExpr;
                ret.AddRange(DoExpr(m.Base));
                ret.Add(new CompletionItem(m.Ident));
            }
            else if (e is NilExpr)
            {
            }
            else if (e is NumberExpr)
            {
            }
            else if (e is StringExpr)
            {
                StringExpr se = e as StringExpr;
            }
            else if (e is TableConstructorStringKeyExpr)
            {
                TableConstructorStringKeyExpr tcske = e as TableConstructorStringKeyExpr;
                ret.Add(new CompletionItem(tcske.Key));
                ret.AddRange(DoExpr(tcske.Value));
            }
            else if (e is TableConstructorExpr)
            {
                TableConstructorExpr t = e as TableConstructorExpr;
                for (int i = 0; i < t.EntryList.Count; i++)
                    ret.AddRange(DoExpr(t.EntryList[i]));
            }
            else if (e is UnOpExpr)
            {
            }
            else if (e is TableConstructorValueExpr)
                ret.AddRange(DoExpr((e as TableConstructorValueExpr).Value));
            else if (e is VarargExpr)
            {
            }
            else if (e is VariableExpression)
                ret.Add(new CompletionItem((e as VariableExpression).Var.Name));

            return ret;
        }

        static List<CompletionItem> DoStatement(Statement s)
        {
            List<CompletionItem> ret = new List<CompletionItem>();
            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                foreach (Expression e in a.Lhs)
                    ret.AddRange(DoExpr(e));
                foreach (Expression e in a.Rhs)
                    ret.AddRange(DoExpr(e));
            }
            else if (s is AugmentedAssignmentStatement)
            {
                AugmentedAssignmentStatement a = s as AugmentedAssignmentStatement;
                foreach (Expression e in a.Lhs)
                    ret.AddRange(DoExpr(e));
                foreach (Expression e in a.Rhs)
                    ret.AddRange(DoExpr(e));
            }
            else if (s is BreakStatement)
            {
            }
            else if (s is CallStatement)
            {
                // Also incredibly simple...
                CallStatement c = s as CallStatement;
                return DoExpr(c.Expression);
            }
            else if (s is DoStatement)
            {
                DoStatement d = s as DoStatement;
                return DoChunk(d.Body);
            }
            else if (s is GenericForStatement)
            {
                GenericForStatement g = s as GenericForStatement;

                foreach (Variable v in g.VariableList)
                    ret.Add(new CompletionItem(v.Name));
                ret.AddRange(DoChunk(g.Body));
            }
            else if (s is NumericForStatement)
            {
                NumericForStatement n = s as NumericForStatement;
                ret.Add(new CompletionItem(n.Variable.Name));
                ret.AddRange(DoChunk(n.Body));
            }
            else if (s is FunctionStatement)
            {
                FunctionStatement f = s as FunctionStatement;
                ret.AddRange(DoExpr(f.Name));
                foreach (Variable v in f.Arguments)
                    ret.Add(new CompletionItem(v.Name));
                ret.AddRange(DoChunk(f.Body));
            }
            else if (s is GotoStatement)
            {
            }
            else if (s is IfStmt)
            {
                IfStmt i = s as IfStmt;
                for (int x = 0; x < i.Clauses.Count; x++)
                    ret.AddRange(DoStatement(i.Clauses[x]));
            }
            else if (s is LabelStatement)
            {
            }
            else if (s is RepeatStatement)
            {
                RepeatStatement r = s as RepeatStatement;
                ret.AddRange(DoChunk(r.Body));
                ret.AddRange(DoExpr(r.Condition));
            }
            else if (s is ReturnStatement)
            {
                // no variable defined here. hopefully.
            }
            else if (s is UsingStatement)
            {
                UsingStatement u = s as UsingStatement;
                ret.AddRange(DoStatement(u.Vars));
                ret.AddRange(DoChunk(u.Body));
            }
            else if (s is WhileStatement)
            {
                WhileStatement w = s as WhileStatement;
                ret.AddRange(DoExpr(w.Condition));
                ret.AddRange(DoChunk(w.Body));
            }

            else if (s is ElseIfStmt)
            {
                ElseIfStmt e = s as ElseIfStmt;
                return DoChunk(e.Body);
            }
            else if (s is ElseStmt)
            {
                return DoChunk(((ElseStmt)s).Body);
            }

            return ret;
        }

        static List<CompletionItem> DoChunk(List<Statement> statements)
        {
            List<CompletionItem> ret = new List<CompletionItem>();
            for (int i = 0; i < statements.Count; i++)
            {
                ret.AddRange(DoStatement(statements[i]));

            }
            return ret;
        }
    }
}
