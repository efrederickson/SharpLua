using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;

namespace SharpLua.Visitors
{
    // Same base as ExactReconstructor, except it doesn't extract whitespace from tokens

    public class NonModifiedAstBeautifier
    {
        BasicBeautifier beautifier = new BasicBeautifier();
        public FormattingOptions options = new FormattingOptions();

        internal int indent = 0;
        string nlindent()
        {
            indent++;
            return options.EOL + writeIndent();
        }

        string nldedent()
        {
            indent--;
            return options.EOL + writeIndent();
        }

        string nl()
        {
            return options.EOL + writeIndent();
        }

        string writeIndent()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < indent; i++)
                s.Append(options.Tab);
            return s.ToString();
        }

        string fromToken(Token t, Scope s)
        {
            StringBuilder sb = new StringBuilder();
            int cnt = 0;
            bool shortComment = false;
            int i = 0;
            foreach (Token t2 in t.Leading)
            {
                if (t2.Type == TokenType.LongComment
                    || t2.Type == TokenType.ShortComment
                    || t2.Type == TokenType.DocumentationComment)
                {
                    sb.Append(t2.Data);
                    cnt++;
                    if (t2.Type == TokenType.ShortComment || t2.Type == TokenType.DocumentationComment || (t.Leading.Count >= i + 1 && (t.Leading[i + 1].Type == TokenType.WhitespaceR || t.Leading[i + 1].Type == TokenType.WhitespaceN)))
                    {
                        shortComment = true;
                        sb.Append(options.EOL);
                    }
                }
                i++;
            }
            if (cnt > 0)
            {
                if (shortComment && (sb[sb.Length - 1] != '\r' || sb[sb.Length - 1] != '\n'))
                {
                    sb.Append(options.EOL);
                    sb.Append(writeIndent());
                }
                else
                {
                    sb.Insert(0, " ");
                    sb.Append(" ");
                }
            }

            if (t.Type == TokenType.DoubleQuoteString)
                sb.Append("\"" + t.Data + "\"");
            else if (t.Type == TokenType.SingleQuoteString)
                sb.Append("'" + t.Data + "'");
            else if (t.Type == TokenType.Ident)
            {
                Variable v = s.GetOldVariable(t.Data);
                if (v != null)
                    sb.Append(v.Name);
                else
                    sb.Append(t.Data);
            }
            else
                sb.Append(t.Data);


            if (t.FollowingEoSToken != null && t.FollowingEoSToken.Type == TokenType.EndOfStream)
                sb.Append(fromToken(t.FollowingEoSToken, s));
            return sb.ToString();
        }

        string fromTokens(List<Token> tokens, Scope s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token t in tokens)
                sb.Append(fromToken(t, s));
            return sb.ToString();
        }

        List<Token> tok = null;
        int index = 0;

        internal string DoExpr(Expression e, Scope s)
        {
            int startP = index;
            for (int i = 0; i < e.ParenCount; i++)
                index++;

            string ret = null;
            if (e is AnonymousFunctionExpr) // function() ... end
            {
                AnonymousFunctionExpr f = e as AnonymousFunctionExpr;
                StringBuilder sb = new StringBuilder();

                sb.Append(fromToken(tok[index++], s)); // 'function' 
                sb.Append(fromToken(tok[index++], s)); // '('
                for (int i2 = 0; i2 < f.Arguments.Count; i2++)
                {
                    sb.Append(fromToken(tok[index++], s));
                    if (i2 != f.Arguments.Count - 1 || f.IsVararg)
                        sb.Append(fromToken(tok[index++], s) + " ");
                }
                if (f.IsVararg)
                    sb.Append(fromToken(tok[index++], s));
                sb.Append(fromToken(tok[index++], s)); // ')'

                if (f.Body.Count > 1)
                {
                    sb.Append(options.EOL);
                    indent++;
                    sb.Append(DoChunk(f.Body));
                    sb.Append(nldedent());
                }
                else if (f.Body.Count == 0)
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(" " + DoStatement(f.Body[0]));
                    sb.Append(" ");
                }

                //sb.Append(DoChunk(f.Body));
                // Ugh.
                sb.Append(fromToken(tok[index++], s)); // <end>

                ret = sb.ToString();
            }
            else if (e is BinOpExpr)
            {
                //int i = 0;
                string left = DoExpr((e as BinOpExpr).Lhs, s);
                string op = fromToken(tok[index++], s);
                string right = DoExpr((e as BinOpExpr).Rhs, s);
                ret = string.Format("{0} {1} {2}", left, op, right);
            }
            else if (e is BoolExpr)
            {
                ret = fromToken(tok[index++], s);
            }
            else if (e is CallExpr && (!(e is StringCallExpr) && !(e is TableCallExpr)))
            {
                CallExpr c = e as CallExpr;
                StringBuilder sb = new StringBuilder();
                sb.Append(DoExpr(c.Base, s) // <base>
                    + fromToken(tok[index++], s)); // '('
                for (int i = 0; i < c.Arguments.Count; i++)
                {
                    sb.Append(DoExpr(c.Arguments[i], s));
                    if (i != c.Arguments.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s)); // ', '
                        sb.Append(" ");
                    }
                }
                sb.Append(fromToken(tok[index++], s)); // ')'
                ret = sb.ToString();
            }
            else if (e is StringCallExpr)
            {
                StringCallExpr sc = e as StringCallExpr;
                ret = string.Format("{0} {1}", DoExpr(sc.Base, s), DoExpr(sc.Arguments[0], s));
            }
            else if (e is TableCallExpr)
            {
                TableCallExpr sc = e as TableCallExpr;
                ret = string.Format("{0} {1}", DoExpr(sc.Base, s), DoExpr(sc.Arguments[0], s));
            }
            else if (e is IndexExpr)
            {
                IndexExpr i = e as IndexExpr;
                ret = string.Format("{0}{1}{2}{3}", DoExpr(i.Base, s), fromToken(tok[index++], s), DoExpr(i.Index, s), fromToken(tok[index++], s));
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
                InlineFunctionExpression ife = e as InlineFunctionExpression;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s)); // '|;
                for (int i = 0; i < ife.Arguments.Count; i++)
                {
                    sb.Append(fromToken(tok[index++], s)); // <arg name>
                    if (i != ife.Arguments.Count - 1 || ife.IsVararg)
                    {
                        sb.Append(fromToken(tok[index++], s)); // ','
                        sb.Append(" ");
                    }
                }
                if (ife.IsVararg)
                {
                    sb.Append(fromToken(tok[index++], s)); // '...'
                    sb.Append(" ");
                }
                sb.Append(fromToken(tok[index++], s)); // '|'
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s)); // '->'
                sb.Append(" ");
                for (int i2 = 0; i2 < ife.Expressions.Count; i2++)
                {
                    sb.Append(DoExpr(ife.Expressions[i2], s));
                    if (i2 != ife.Expressions.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s)); // ','
                        sb.Append(" ");
                    }
                }
                ret = sb.ToString();
            }
            else if (e is TableConstructorKeyExpr)
            {
                TableConstructorKeyExpr t = e as TableConstructorKeyExpr;
                ret =
                    fromToken(tok[index++], s)
                    + DoExpr(t.Key, s)
                    + fromToken(tok[index++], s)
                    + " "
                    + fromToken(tok[index++], s)
                    + " "
                    + DoExpr(t.Value, s);
            }
            else if (e is MemberExpr)
            {
                MemberExpr m = e as MemberExpr;
                ret = DoExpr(m.Base, s) + fromToken(tok[index++], s) + fromToken(tok[index++], s);
            }
            else if (e is NilExpr)
                ret = fromToken(tok[index++], s);
            else if (e is NumberExpr)
                ret = fromToken(tok[index++], s);
            else if (e is StringExpr)
                ret = fromToken(tok[index++], s);
            else if (e is TableConstructorStringKeyExpr)
            {
                TableConstructorStringKeyExpr tcske = e as TableConstructorStringKeyExpr;
                ret = fromToken(tok[index++], s); // key
                ret += " ";
                ret += fromToken(tok[index++], s); // '='
                ret += " ";
                ret += DoExpr(tcske.Value, s); // value
            }
            else if (e is TableConstructorExpr)
            {
                TableConstructorExpr t = e as TableConstructorExpr;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s)); // '{'
                sb.Append(" ");
                for (int i = 0; i < t.EntryList.Count; i++)
                {
                    sb.Append(DoExpr(t.EntryList[i], s));
                    if (i != t.EntryList.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s)); // ','
                        sb.Append(" ");
                    }
                }
                if (t.EntryList.Count > 0) // empty table constructor is just { }
                    sb.Append(" ");
                sb.Append(fromToken(tok[index++], s)); // '}'
                ret = sb.ToString();
            }
            else if (e is UnOpExpr)
            {
                UnOpExpr u = e as UnOpExpr;
                string sc = fromToken(tok[index++], s);
                if (u.Op.Length != 1)
                    sc += " ";
                ret = sc + DoExpr(u.Rhs, s);
            }
            else if (e is TableConstructorValueExpr)
                ret = DoExpr(((TableConstructorValueExpr)e).Value, s);
            else if (e is VarargExpr)
                ret = fromToken(tok[index++], s);
            else if (e is VariableExpression)
                ret = fromToken(tok[index++], s);
            else if (e is TableConstructorNamedFunctionExpr)
                ret = DoStatement(((TableConstructorNamedFunctionExpr)e).Value);

            if (ret != null)
            {
                if (e.ParenCount > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < e.ParenCount; i++)
                        sb.Append(fromToken(tok[startP++], s));
                    sb.Append(ret);
                    for (int i = 0; i < e.ParenCount; i++)
                        sb.Append(fromToken(tok[index++], s));
                    return sb.ToString();
                }
                else
                    return ret;
            }
            //return string.Format("{0}{1}{2}", "(".Repeat(e.ParenCount), ret, ")".Repeat(e.ParenCount));

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }

        internal string DoStatement(Statement s)
        {
            // If the statement contains a body, we cant just fromTokens it, as it's body might not be 
            // fully tokenized input. Therefore, we run DoChunk on Body's

            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                StringBuilder sb = new StringBuilder();
                if (a.IsLocal)
                {
                    sb.Append(fromToken(tok[index++], a.Scope));
                    sb.Append(" ");
                }
                for (int i2 = 0; i2 < a.Lhs.Count; i2++)
                {
                    sb.Append(DoExpr(a.Lhs[i2], s.Scope));
                    if (i2 != a.Lhs.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], a.Scope));
                        sb.Append(" ");
                    }
                }
                if (a.Rhs.Count > 0)
                {
                    sb.Append(" ");
                    sb.Append(fromToken(tok[index++], a.Scope));
                    sb.Append(" ");
                    for (int i2 = 0; i2 < a.Rhs.Count; i2++)
                    {
                        sb.Append(DoExpr(a.Rhs[i2], s.Scope));
                        if (i2 != a.Rhs.Count - 1)
                        {
                            sb.Append(fromToken(tok[index++], s.Scope));
                            sb.Append(" ");
                        }
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
                {
                    sb.Append(fromToken(tok[index++], a.Scope));
                    sb.Append(" ");
                }
                Expression assignment = ((BinOpExpr)a.Rhs[0]).Rhs;
                Expression tmp = ((BinOpExpr)a.Rhs[0]).Lhs;
                sb.Append(DoExpr(tmp, s.Scope));
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope));
                sb.Append(" ");
                sb.Append(DoExpr(assignment, s.Scope));
                return sb.ToString();
            }
            else if (s is BreakStatement)
            {
                // HAHAHA this is incredibly simple...
                return fromToken(tok[index++], s.Scope); // 'break'
            }
            else if (s is ContinueStatement)
            {
                return fromToken(tok[index++], s.Scope); // 'continue'
            }
            else if (s is CallStatement)
            {
                // Also incredibly simple...
                return DoExpr(((CallStatement)s).Expression, s.Scope);
            }
            else if (s is DoStatement)
            {
                DoStatement d = s as DoStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'do'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(d.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], s.Scope)); // end
                return sb.ToString();
            }
            else if (s is GenericForStatement)
            {
                GenericForStatement g = s as GenericForStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'for'
                sb.Append(" ");
                for (int x = 0; x < g.VariableList.Count; x++)
                {
                    sb.Append(fromToken(tok[index++], s.Scope));
                    if (x != g.VariableList.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s.Scope)); // ','
                        sb.Append(" ");
                    }
                }
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // 'in'
                sb.Append(" ");
                for (int x = 0; x < g.Generators.Count; x++)
                {
                    //sb.Append(fromToken(tok[index++], s.Scope));
                    DoExpr(g.Generators[x], s.Scope);
                    if (x != g.VariableList.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s.Scope)); // ','
                        sb.Append(" ");
                    }
                }
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // 'do'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(g.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], s.Scope)); // <end>
                return sb.ToString();
            }
            else if (s is NumericForStatement)
            {
                NumericForStatement n = s as NumericForStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'for'
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // <var>
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // '='
                sb.Append(" ");
                sb.Append(DoExpr(n.Start, s.Scope)); // <start>
                //sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // ','
                sb.Append(" ");
                sb.Append(DoExpr(n.End, s.Scope)); // <end>
                if (n.Step != null)
                {
                    sb.Append(fromToken(tok[index++], s.Scope)); // ','
                    sb.Append(" ");
                    sb.Append(DoExpr(n.Step, s.Scope)); // <step>
                    sb.Append(" ");
                }
                else
                    sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // 'do'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(n.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], s.Scope)); // <end>
                return sb.ToString();
            }
            else if (s is FunctionStatement)
            {
                FunctionStatement f = s as FunctionStatement;
                StringBuilder sb = new StringBuilder();

                sb.Append(fromToken(tok[index++], s.Scope)); // 'function' 
                sb.Append(" ");
                sb.Append(DoExpr(f.Name, s.Scope));
                sb.Append(fromToken(tok[index++], s.Scope)); // '('
                for (int i2 = 0; i2 < f.Arguments.Count; i2++)
                {
                    sb.Append(fromToken(tok[index++], s.Scope));
                    if (i2 != f.Arguments.Count - 1)// || f.IsVararg)
                    {
                        sb.Append(fromToken(tok[index++], s.Scope));
                        sb.Append(" ");
                    }
                }
                if (f.IsVararg)
                {
                    sb.Append(fromToken(tok[index++], s.Scope));
                    sb.Append(" ");
                    sb.Append(fromToken(tok[index++], s.Scope));
                }
                sb.Append(fromToken(tok[index++], s.Scope)); // ')'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(f.Body));
                indent--;
                sb.Append(options.EOL);
                sb.Append(writeIndent());
                sb.Append(fromToken(tok[index++], s.Scope)); // <end>

                sb.Append(options.EOL);

                return sb.ToString();
            }
            else if (s is GotoStatement)
            {
                // goto <string label>, so no expr
                //GotoStatement g = s as GotoStatement;
                return fromToken(tok[index++], s.Scope) + fromToken(tok[index++], s.Scope);
            }
            else if (s is IfStmt)
            {
                IfStmt ifs = s as IfStmt;
                StringBuilder sb = new StringBuilder();

                foreach (SubIfStmt clause in ifs.Clauses)
                {
                    if (clause is ElseIfStmt)
                    {
                        ElseIfStmt c = clause as ElseIfStmt;

                        sb.Append(fromToken(tok[index++], s.Scope)); // if/elseif
                        sb.Append(" ");
                        sb.Append(DoExpr(c.Condition, c.Scope));
                        sb.Append(" ");
                        sb.Append(fromToken(tok[index++], s.Scope)); // 'then'
                        sb.Append(options.EOL);
                        indent++;
                        sb.Append(DoChunk(clause.Body));
                        sb.Append(nldedent());
                    }
                    else if (clause is ElseStmt)
                    {
                        sb.Append(fromToken(tok[index++], s.Scope)); // if/elseif
                        sb.Append(options.EOL);
                        indent++;
                        sb.Append(DoChunk(clause.Body));
                        sb.Append(nldedent());
                    }
                    else
                        throw new NotImplementedException(clause.GetType().Name);
                }
                sb.Append(fromToken(tok[index++], s.Scope)); // 'end'
                return sb.ToString();
            }
            else if (s is LabelStatement)
            {
                // ::<string label>::, so no expr
                return fromToken(tok[index++], s.Scope) // '::'
                    + fromToken(tok[index++], s.Scope)  // label
                    + fromToken(tok[index++], s.Scope); //'::'
            }
            else if (s is RepeatStatement)
            {
                RepeatStatement r = s as RepeatStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope));
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(r.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], r.Scope));
                sb.Append(" ");
                sb.Append(DoExpr(r.Condition, r.Scope));
                return sb.ToString();
            }
            else if (s is ReturnStatement)
            {
                ReturnStatement rs = s as ReturnStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'return'
                sb.Append(" ");
                for (int x = 0; x < rs.Arguments.Count; x++)
                {
                    sb.Append(DoExpr(rs.Arguments[x], s.Scope));
                    if (x != rs.Arguments.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], s.Scope)); // ','
                        sb.Append(" ");
                    }
                }
                return sb.ToString();
            }
            else if (s is UsingStatement)
            {
                UsingStatement u = s as UsingStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'using'
                sb.Append(" ");

                AssignmentStatement a = u.Vars;
                for (int i2 = 0; i2 < a.Lhs.Count; i2++)
                {
                    sb.Append(DoExpr(a.Lhs[i2], s.Scope));
                    if (i2 != a.Lhs.Count - 1)
                    {
                        sb.Append(fromToken(tok[index++], a.Scope));
                        sb.Append(" ");
                    }
                }
                if (a.Rhs.Count > 0)
                {
                    sb.Append(" ");
                    sb.Append(fromToken(tok[index++], a.Scope)); // '='
                    sb.Append(" ");
                    for (int i2 = 0; i2 < a.Rhs.Count; i2++)
                    {
                        sb.Append(DoExpr(a.Rhs[i2], s.Scope));
                        if (i2 != a.Rhs.Count - 1)
                        {
                            sb.Append(fromToken(tok[index++], s.Scope));
                            sb.Append(" ");
                        }
                    }
                }
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // 'do'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(u.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], s.Scope)); // 'end'
                return sb.ToString();
            }
            else if (s is WhileStatement)
            {
                WhileStatement w = s as WhileStatement;
                StringBuilder sb = new StringBuilder();
                sb.Append(fromToken(tok[index++], s.Scope)); // 'while'
                sb.Append(" ");
                sb.Append(DoExpr(w.Condition, s.Scope));
                sb.Append(" ");
                sb.Append(fromToken(tok[index++], s.Scope)); // 'do'
                sb.Append(options.EOL);
                indent++;
                sb.Append(DoChunk(w.Body));
                sb.Append(nldedent());
                sb.Append(fromToken(tok[index++], s.Scope));
                return sb.ToString();
            }
            else
                throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        internal string DoChunk(List<Statement> stmts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stmts.Count; i++)
            {
                Statement s = stmts[i];
                sb.Append(writeIndent());
                //string ss = DoStatement(s, ref index);
                string ss = DoStatement(s);
                sb.Append(ss);
                if (ss.EndsWith(options.EOL) == false && i != stmts.Count - 1)
                    sb.Append(options.EOL);

                if (s.HasSemicolon)
                    if (s.SemicolonToken != null)
                        sb.Append(fromToken(s.SemicolonToken, s.Scope));
                    else
                        sb.Append(";");
            }
            return sb.ToString();
        }

        public string Beautify(Chunk c)
        {
            tok = c.ScannedTokens;
            index = 0;
            return DoChunk(c.Body);
        }
    }
}
