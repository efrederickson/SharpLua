using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Statement;
using SharpLua.Ast.Expression;

namespace SharpLua
{
    // TODO:
    // Getting function names might need to be better

    public class DocumentationComment
    {
        public List<string> Lines = new List<string>();
        public Token Ident = null;
        public string EOL = "\r\n";

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string l in Lines)
                {
                    string line = l.TrimStart();
                    if (line.Substring(0, 3) == "---")
                        line = line.Substring(3);
                    sb.Append(line);
                    sb.Append(EOL);
                }
                return sb.ToString();
            }
        }

    }

    public class ExtractDocumentationComments
    {
        public static List<DocumentationComment> Extract(Chunk c)
        {
            if (c.ScannedTokens != null && c.ScannedTokens.Count > 0)
            {
                List<DocumentationComment> cmnts = new List<DocumentationComment>();
                for (int p = 0; p < c.ScannedTokens.Count; p++)
                {
                    Token t_ = c.ScannedTokens[p];
                    Token t = t_.Leading.Count > 0 ? t_.Leading[0] : null;
                    if (t != null)
                    {
                        int i = 0;
                        DocumentationComment cmt = new DocumentationComment();
                        do
                        {
                            if (t_.Leading.Count <= i)
                                break;
                            t = t_.Leading[i++];
                            if (t.Type == TokenType.DocumentationComment)
                                cmt.Lines.Add(t.Data);
                        } while (t.Type == TokenType.WhitespaceN
                            || t.Type == TokenType.WhitespaceR
                            || t.Type == TokenType.WhitespaceSpace
                            || t.Type == TokenType.WhitespaceTab
                            || t.Type == TokenType.DocumentationComment);

                        // find the ident it's for
                        if (c.ScannedTokens.Count > p)
                        {
                            t = c.ScannedTokens[p];
                            if (t.Type == TokenType.Keyword && t.Data == "local")
                                if (c.ScannedTokens[p + 1].Type == TokenType.Keyword && c.ScannedTokens[p + 1].Data == "function")
                                {
                                    int i2 = 2;
                                    while (
                                        (c.ScannedTokens[p + i2].Type == TokenType.Symbol && c.ScannedTokens[p + i2].Data == ".")
                                        || (c.ScannedTokens[p + i2].Type == TokenType.Ident))
                                        i2++;
                                    cmt.Ident = c.ScannedTokens[p + i2 - 1];
                                }
                                else
                                    cmt.Ident = c.ScannedTokens[p + 1];
                            else if (t.Type == TokenType.Keyword && t.Data == "function")
                            {
                                int i2 = 1;
                                while (
                                    (c.ScannedTokens[p + i2].Type == TokenType.Symbol && c.ScannedTokens[p + i2].Data == ".")
                                    || (c.ScannedTokens[p + i2].Type == TokenType.Ident))
                                    i2++;
                                cmt.Ident = c.ScannedTokens[p + i2 - 1];
                            }
                            else if (t.Type == TokenType.Ident)
                                cmt.Ident = t;

                        }

                        cmnts.Add(cmt);
                    }
                }
                return cmnts;
            }
            return new List<DocumentationComment>();
        }
    }
}
