using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua.Ast;

namespace SharpLua.Visitors
{
    // Non-Lua compliant constructs:
    // - augmented operators          -> expanded form (a += 1 -> a = a + (1))
    // - inline functions             -> Anonymous functions
    // - match/with                   -> if/then/elseif
    // (Not implemented, as match/with may be removed soon due to '|' conflicts)
    // - using                        -> do/end

    public class LuaCompatibleOutput
    {
        //StringBuilder sb = new StringBuilder();
        public string EOL = "\r\n";
        public string Tab = "    ";

        int indent = 0;
        string nlindent()
        {
            indent++;
            return EOL + writeIndent();
        }

        string nldedent()
        {
            indent--;
            return EOL + writeIndent();
        }

        string nl()
        {
            return EOL + writeIndent();
        }

        string writeIndent()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < indent; i++)
                s.Append(Tab);
            return s.ToString();
        }

        string oparens(int i)
        {
            return "(".Repeat(i);
        }

        string cparens(int i)
        {
            return ")".Repeat(i);
        }

        string DoExpr(Expression e)
        {
            string ret = "";
            if (e is AnonymousFunctionExpr) // function() ... end
            {
                AnonymousFunctionExpr f = e as AnonymousFunctionExpr;
                StringBuilder sb = new StringBuilder();
                sb.Append("function(");
                for (int i = 0; i < f.Arguments.Count; i++)
                {
                    sb.Append(f.Arguments[i].Name);
                    if (i != f.Arguments.Count - 1 || f.IsVararg)
                        sb.Append(", ");
                }
                if (f.IsVararg)
                    sb.Append("...");
                sb.Append(")");
                if (f.Body.Count > 1)
                {
                    sb.Append(EOL);
                    indent++;
                    sb.Append(DoChunk(f.Body));
                    sb.Append(nldedent());
                    sb.Append("end");
                }
                else if (f.Body.Count == 0)
                {
                    sb.Append(" end");
                }
                else
                {
                    sb.Append(" " + DoStatement(f.Body[0]));
                    sb.Append(" end");
                }

                ret = sb.ToString();
            }
            else if (e is BinOpExpr)
            {
                BinOpExpr b = e as BinOpExpr;
                string left = DoExpr(b.Lhs);
                string op = b.Op;
                string right = DoExpr(b.Rhs);
                ret = string.Format("{0} {1} {2}", left, op, right);
            }
            else if (e is BoolExpr)
            {
                bool val = (e as BoolExpr).Value;
                ret = val ? "true" : "false";
            }
            else if (e is CallExpr && (!(e is StringCallExpr) && !(e is TableCallExpr)))
            {
                CallExpr c = e as CallExpr;
                StringBuilder sb = new StringBuilder();
                sb.Append(DoExpr(c.Base) + "(");
                for (int i = 0; i < c.Arguments.Count; i++)
                {
                    sb.Append(DoExpr(c.Arguments[i]));
                    if (i != c.Arguments.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(")");
                ret = sb.ToString();
            }
            else if (e is StringCallExpr)
            {
                StringCallExpr s = e as StringCallExpr;
                ret = string.Format("{0} {1}", DoExpr(s.Base), DoExpr(s.Arguments[0]));
            }
            else if (e is TableCallExpr)
            {
                TableCallExpr s = e as TableCallExpr;
                ret = string.Format("{0} {1}", DoExpr(s.Base), DoExpr(s.Arguments[0]));
            }
            else if (e is IndexExpr)
            {
                IndexExpr i = e as IndexExpr;
                ret = string.Format("{0}[{1}]", DoExpr(i.Base), DoExpr(i.Index));
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
                InlineFunctionExpression ife = e as InlineFunctionExpression;
                StringBuilder sb = new StringBuilder();
                sb.Append("function(");
                for (int i = 0; i < ife.Arguments.Count; i++)
                {
                    sb.Append(ife.Arguments[i].Name);
                    if (i != ife.Arguments.Count - 1 || ife.IsVararg)
                        sb.Append(", ");
                }
                if (ife.IsVararg)
                    sb.Append("...");
                sb.Append(") return ");
                for (int i2 = 0; i2 < ife.Expressions.Count; i2++)
                {
                    sb.Append(DoExpr(ife.Expressions[i2]));
                    if (i2 != ife.Expressions.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(" end");
                ret = sb.ToString();
            }
            else if (e is TableConstructorKeyExpr)
            {
                TableConstructorKeyExpr t = e as TableConstructorKeyExpr;
                ret = "[" + DoExpr(t.Key) + "] = " + DoExpr(t.Value);
            }
            else if (e is MemberExpr)
            {
                MemberExpr m = e as MemberExpr;
                ret = DoExpr(m.Base) + m.Indexer + m.Ident;
            }
            else if (e is NilExpr)
                ret = "nil";
            else if (e is NumberExpr)
                ret = (e as NumberExpr).Value;
            else if (e is StringExpr)
            {
                StringExpr se = e as StringExpr;
                string delim = se.StringType == TokenType.SingleQuoteString ? "'" : se.StringType == TokenType.DoubleQuoteString ? "\"" : "[";
                if (delim == "[")
                {
                    // Long strings keep their [=*[
                    ret = se.Value;
                }
                else
                    ret = delim + se.Value + delim;
            }
            else if (e is TableConstructorStringKeyExpr)
            {
                TableConstructorStringKeyExpr tcske = e as TableConstructorStringKeyExpr;
                ret = tcske.Key + " = " + DoExpr(tcske.Value);
            }
            else if (e is TableConstructorExpr)
            {
                TableConstructorExpr t = e as TableConstructorExpr;
                StringBuilder sb = new StringBuilder();
                sb.Append("{ ");
                for (int i = 0; i < t.EntryList.Count; i++)
                {
                    sb.Append(DoExpr(t.EntryList[i]));
                    if (i != t.EntryList.Count - 1)
                        sb.Append(", ");
                }
                sb.Append("} ");
                ret = sb.ToString();
            }
            else if (e is UnOpExpr)
            {
                string op = (e as UnOpExpr).Op;
                string s = op;
                if (s.Length != 1)
                    s += " ";
                ret = s + DoExpr((e as UnOpExpr).Rhs);
            }
            else if (e is TableConstructorValueExpr)
                ret = DoExpr((e as TableConstructorValueExpr).Value);
            else if (e is VarargExpr)
                ret = "...";
            else if (e is VariableExpression)
                ret = (e as VariableExpression).Name;

            return string.Format("{0}{1}{2}", oparens(e.ParenCount), ret, cparens(e.ParenCount));

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }

        string DoStatement(Statement s)
        {
            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                StringBuilder sb = new StringBuilder();
                if (a.IsLocal)
                    sb.Append("local ");
                for (int i = 0; i < a.Lhs.Count; i++)
                {
                    sb.Append(DoExpr(a.Lhs[i]));
                    if (i != a.Lhs.Count - 1)
                        sb.Append(", ");
                }
                if (a.Rhs.Count > 0)
                {
                    sb.Append(" = ");
                    for (int i = 0; i < a.Rhs.Count; i++)
                    {
                        sb.Append(DoExpr(a.Rhs[i]));
                        if (i != a.Rhs.Count - 1)
                            sb.Append(", ");
                    }
                }
                return sb.ToString();
            }
            else if (s is AugmentedAssignmentStatement)
            {
                AugmentedAssignmentStatement a = s as AugmentedAssignmentStatement;
                StringBuilder sb = new StringBuilder();
                //sb.Append(DoExpr(a.Lhs[0]));
                if (a.IsLocal)
                    sb.Append("local ");
                sb.Append(DoExpr(a.Lhs[0]));
                sb.Append(" = ");
                sb.Append(DoExpr(a.Lhs[0]));
                sb.Append(" " + ((BinOpExpr)a.Rhs[0]).Op + " ");
                Expression assignment = ((BinOpExpr)a.Rhs[0]).Rhs;
                //sb.Append(DoExpr((((BinOpExpr)a.Rhs[0]).Lhs)));
                //sb.Append(((BinOpExpr)a.Rhs[0]).Op);
                // it might mess up Order of Operations, so we need parens.
                // x *= 6 + 2 != x = x * 6 + 2
                sb.Append("(" + DoExpr(assignment) + ")");
                return sb.ToString();
            }
            else if (s is BreakStatement)
            {
                // HAHAHA this is incredibly simple...
                return "break";
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
                StringBuilder sb = new StringBuilder();
                sb.Append("do" + EOL);
                indent++;
                sb.Append(DoChunk(d.Body));
                sb.Append(nldedent() + "end");
                return sb.ToString();
            }
            else if (s is GenericForStatement)
            {
                GenericForStatement g = s as GenericForStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("for ");
                for (int i = 0; i < g.VariableList.Count; i++)
                {
                    sb.Append(g.VariableList[i].Name);
                    if (i != g.VariableList.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(" in ");
                for (int i = 0; i < g.Generators.Count; i++)
                {
                    sb.Append(DoExpr(g.Generators[i]));
                    if (i != g.Generators.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(" do" + EOL);
                indent++;
                sb.Append(DoChunk(g.Body));
                sb.Append(nldedent() + "end");
                return sb.ToString();
            }
            else if (s is NumericForStatement)
            {
                NumericForStatement n = s as NumericForStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("for ");
                sb.Append(n.Variable.Name);
                sb.Append(" = ");
                sb.Append(DoExpr(n.Start));
                sb.Append(", ");
                sb.Append(DoExpr(n.End));
                if (n.Step != null)
                {
                    sb.Append(", ");
                    sb.Append(DoExpr(n.Step));
                }
                sb.Append(" do" + EOL);
                indent++;
                sb.Append(DoChunk(n.Body));
                sb.Append(nldedent() + "end");
                return sb.ToString();
            }
            else if (s is FunctionStatement)
            {
                FunctionStatement f = s as FunctionStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("function " + DoExpr(f.Name) + "(");
                for (int i = 0; i < f.Arguments.Count; i++)
                {
                    sb.Append(f.Arguments[i].Name);
                    if (i != f.Arguments.Count - 1 || f.IsVararg)
                        sb.Append(", ");
                }
                if (f.IsVararg)
                    sb.Append("...");
                sb.Append(")");
                if (f.Body.Count > 1)
                {
                    sb.Append(EOL);
                    indent++;
                    sb.Append(DoChunk(f.Body));
                    sb.Append(nldedent());
                    sb.Append("end");
                }
                else if (f.Body.Count == 0)
                {
                    sb.Append(" end");
                }
                else
                {
                    sb.Append(" " + DoStatement(f.Body[0]));
                    sb.Append(" end");
                }

                return sb.ToString();
            }
            else if (s is GotoStatement)
            {
                GotoStatement g = s as GotoStatement;
                return "goto " + g.Label;
            }
            else if (s is IfStmt)
            {
                IfStmt i = s as IfStmt;
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < i.Clauses.Count; x++)
                {
                    string ind = writeIndent();
                    indent++;
                    string ss = DoStatement(i.Clauses[x]);
                    if (x == 0)
                    {
                        sb.Append(ind + "if ");
                        sb.Append(ss);
                    }
                    else if (i.Clauses[x] is ElseStmt)
                    {
                        sb.Append(ind + "else" + EOL);
                        sb.Append(ss);
                    }
                    else
                        sb.Append(ind + "elseif " + ss);
                    if (x != i.Clauses.Count - 1)
                        sb.Append(EOL);
                    indent--;
                }
                sb.Append(EOL + writeIndent() + "end");
                return sb.ToString();
            }
            else if (s is LabelStatement)
            {
                LabelStatement l = s as LabelStatement;
                return "::" + l.Label + "::";
            }
            else if (s is RepeatStatement)
            {
                RepeatStatement r = s as RepeatStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("repeat");
                if (r.Body.Count == 1)
                {
                    sb.Append(" " + DoStatement(r.Body[0]) + " ");
                }
                else
                {
                    sb.Append(EOL);
                    indent++;
                    sb.Append(DoChunk(r.Body));
                    indent--;
                }
                sb.Append("until " + DoExpr(r.Condition));
                return sb.ToString();
            }
            else if (s is ReturnStatement)
            {
                ReturnStatement r = s as ReturnStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("return ");
                for (int i = 0; i < r.Arguments.Count; i++)
                {
                    sb.Append(DoExpr(r.Arguments[i]));
                    if (i != r.Arguments.Count - 1)
                        sb.Append(", ");
                }
                return sb.ToString();
            }
            else if (s is UsingStatement)
            {
                UsingStatement u = s as UsingStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("do" + EOL);
                indent++;
                sb.Append(writeIndent() + "local ");
                sb.Append(DoStatement(u.Vars));
                sb.Append(EOL);
                sb.Append(DoChunk(u.Body));
                indent--;
                sb.Append(EOL + writeIndent() + "end");
                return sb.ToString();
            }
            else if (s is WhileStatement)
            {
                WhileStatement w = s as WhileStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append("while ");
                sb.Append(DoExpr(w.Condition));
                sb.Append(" do");
                if (w.Body.Count == 1)
                {
                    sb.Append(" " + DoStatement(w.Body[0]) + " ");
                }
                else
                {
                    indent++;
                    sb.Append(DoChunk(w.Body));
                    indent--;
                    sb.Append(EOL);
                }
                sb.Append(writeIndent() + "end");
                return sb.ToString();
            }

            else if (s is ElseIfStmt)
            {
                ElseIfStmt e = s as ElseIfStmt;
                string s2 = DoExpr(e.Condition) + " then" + EOL;
                s2 += DoChunk(e.Body);
                return s2;
            }
            else if (s is ElseStmt)
            {
                return DoChunk(((ElseStmt)s).Body);
            }

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        string DoChunk(List<Statement> statements)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < statements.Count; i++)
            {
                sb.Append(writeIndent());
                sb.Append(DoStatement(statements[i]));
                if (statements[i].HasSemicolon)
                    sb.Append(";");
                if (i != statements.Count - 1)
                    sb.Append(EOL);
            }
            return sb.ToString();
        }

        public string Format(Chunk c)
        {
            return DoChunk(c.Body);
        }
    }
}
