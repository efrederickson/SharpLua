using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast.Expression;
using SharpLua.Ast;
using SharpLua.Ast.Statement;

namespace SharpLua.Visitors
{
    /// <summary>
    /// Not entirely exact, it checks variable renaming...
    /// </summary>
    public class ExactReconstruction
    {
        Beautifier beautifier = new Beautifier();

        string fromToken(Token t, Scope s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token t2 in t.Leading)
                sb.Append(t2.Data);

            if (t.Type == TokenType.DoubleQuoteString)
                sb.Append("\"" + t.Data + "\"");
            else if (t.Type == TokenType.SingleQuoteString)
                sb.Append("'" + t.Data + "'");
            else if (t.Type == TokenType.Ident)
            {
                Variable v = s.GetOldLocal(t.Data);
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

        internal string DoStatement(Statement s)
        {
            // If the statement contains a body, we cant just fromTokens it, as it's body might not be 
            // fully tokenized input. Therefore, we run DoChunk on Body's

            if (s.ScannedTokens != null && s.ScannedTokens.Count > 0)
            {
                if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
                {
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is AugmentedAssignmentStatement)
                {
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is BreakStatement)
                {
                    // HAHAHA this is incredibly simple...
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is CallStatement)
                {
                    // Also incredibly simple...
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is DoStatement)
                {
                    DoStatement d = s as DoStatement;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(fromToken(d.ScannedTokens[0], s.Scope)); // 'do'
                    sb.Append(DoChunk(d.Body));
                    sb.Append(fromToken(d.ScannedTokens[d.ScannedTokens.Count - 1], s.Scope));
                    return sb.ToString();
                }
                else if (s is GenericForStatement)
                {
                    GenericForStatement g = s as GenericForStatement;
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    while (g.ScannedTokens[i].Data != "do")
                        sb.Append(fromToken(g.ScannedTokens[i++], s.Scope));
                    sb.Append(fromToken(g.ScannedTokens[i++], s.Scope));
                    sb.Append(DoChunk(g.Body));
                    sb.Append(fromToken(g.ScannedTokens[g.ScannedTokens.Count - 1], s.Scope));
                    return sb.ToString();
                }
                else if (s is NumericForStatement)
                {
                    NumericForStatement n = s as NumericForStatement;
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    while (n.ScannedTokens[i].Data != "do")
                        sb.Append(fromToken(n.ScannedTokens[i++], s.Scope));
                    sb.Append(fromToken(n.ScannedTokens[i++], s.Scope));
                    sb.Append(DoChunk(n.Body));
                    sb.Append(fromToken(n.ScannedTokens[n.ScannedTokens.Count - 1], s.Scope));
                    return sb.ToString();
                }
                else if (s is FunctionStatement)
                {
                    FunctionStatement f = s as FunctionStatement;
                    StringBuilder sb = new StringBuilder();

                    int i = 0;
                    while (f.ScannedTokens[i].Data != ")")
                        sb.Append(fromToken(f.ScannedTokens[i++], s.Scope));
                    sb.Append(fromToken(f.ScannedTokens[i++], s.Scope));

                    sb.Append(DoChunk(f.Body));
                    sb.Append(fromToken(f.ScannedTokens[f.ScannedTokens.Count - 1], s.Scope));

                    return sb.ToString();
                }
                else if (s is GotoStatement)
                {
                    GotoStatement g = s as GotoStatement;
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is IfStmt)
                {
                    IfStmt i = s as IfStmt;
                    StringBuilder sb = new StringBuilder();

                    foreach (SubIfStmt clause in i.Clauses)
                    {
                        int i2 = 0;
                        while (clause.ScannedTokens[i2].Data != "then")
                            sb.Append(fromToken(clause.ScannedTokens[i2++], clause.Scope));
                        sb.Append(DoChunk(clause.Body));
                        sb.Append(fromToken(clause.ScannedTokens[i2++], clause.Scope));
                    }
                    return sb.ToString();
                }
                else if (s is LabelStatement)
                {
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is RepeatStatement)
                {
                    RepeatStatement r = s as RepeatStatement;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(fromToken(r.ScannedTokens[0], s.Scope));
                    sb.Append(DoChunk(r.Body));
                    int i = 0;
                    for (int i2 = 0; i2 < r.ScannedTokens.Count; i2++)
                        if (r.ScannedTokens[i2].Data == "until")
                            i = i2;

                    for (int i2 = i; i2 < r.ScannedTokens.Count; i2++)
                        sb.Append(fromToken(r.ScannedTokens[i2], s.Scope));
                    return sb.ToString();
                }
                else if (s is ReturnStatement)
                {
                    return fromTokens(s.ScannedTokens, s.Scope);
                }
                else if (s is UsingStatement)
                {
                    UsingStatement u = s as UsingStatement;
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    while (s.ScannedTokens[i].Data != "do")
                        sb.Append(fromToken(s.ScannedTokens[i++], s.Scope));
                    sb.Append(fromToken(s.ScannedTokens[i++], s.Scope));

                    sb.Append(DoChunk(u.Body));
                    sb.Append(fromToken(s.ScannedTokens[s.ScannedTokens.Count - 1], s.Scope));
                    return sb.ToString();
                }
                else if (s is WhileStatement)
                {
                    WhileStatement w = s as WhileStatement;
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    while (s.ScannedTokens[i].Data != "do")
                        sb.Append(fromToken(s.ScannedTokens[i++], s.Scope));
                    sb.Append(fromToken(s.ScannedTokens[i++], s.Scope));

                    sb.Append(DoChunk(w.Body));
                    sb.Append(fromToken(s.ScannedTokens[s.ScannedTokens.Count - 1], s.Scope));
                    return sb.ToString();
                }
                /*
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
                */
            }
            else // No token stream, beautify
                return beautifier.DoStatement(s);

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        internal string DoChunk(List<Statement> statements)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Statement s in statements)
                sb.Append(DoStatement(s));
            return sb.ToString();
        }

        public string Reconstruct(Chunk c)
        {
            return DoChunk(c.Body);
        }
    }
}
