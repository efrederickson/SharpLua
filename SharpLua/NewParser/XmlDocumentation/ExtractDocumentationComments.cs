using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Statement;
using SharpLua.Ast.Expression;

namespace SharpLua.XmlDocumentation
{
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
                                    cmt.Ident = c.ScannedTokens[p + i2 - 1].Data;
                                    p += i2;
                                }
                                else
                                {
                                    int i2 = 2;
                                    while (
                                        (c.ScannedTokens[p + i2].Type == TokenType.Symbol && c.ScannedTokens[p + i2].Data == ".")
                                        || (c.ScannedTokens[p + i2].Type == TokenType.Ident))
                                        i2++;
                                    cmt.Ident = c.ScannedTokens[p + i2 - 1].Data;
                                    p += i2;
                                }
                            else if (t.Type == TokenType.Keyword && t.Data == "function")
                            {
                                int i2 = 1;
                                while (
                                    (c.ScannedTokens[p + i2].Type == TokenType.Symbol && c.ScannedTokens[p + i2].Data == ".")
                                    || (c.ScannedTokens[p + i2].Type == TokenType.Ident))
                                    i2++;
                                cmt.Ident = c.ScannedTokens[p + i2 - 1].Data;
                                p += i2;
                            }
                            else if (t.Type == TokenType.Ident)
                            {
                                int i2 = 1;
                                while (
                                    (c.ScannedTokens[p + i2].Type == TokenType.Symbol && c.ScannedTokens[p + i2].Data == ".")
                                    || (c.ScannedTokens[p + i2].Type == TokenType.Ident))
                                    i2++;
                                cmt.Ident = c.ScannedTokens[p + i2 - 1].Data;
                                p += i2;
                            }

                        }
                        if (cmt.Ident != null
                            && string.IsNullOrEmpty(cmt.Ident) == false
                            && cmt.Lines.Count > 0)
                        {
                            //Console.WriteLine("YEP: " + cmt.Ident);
                            cmnts.Add(cmt);
                        }
                        else
                        {
                            /*
                            Console.Write("NOPE: (" + (cmt.Ident == null ? "" : cmt.Ident) + ")");
                            Console.WriteLine(
                                cmt.Ident == null ? "Ident is null"
                                : (string.IsNullOrEmpty(cmt.Ident)
                                  ? "Ident is empty"
                                  : (cmt.Lines.Count == 0
                                    ? "No doc lines"
                                    : "wut?"))
                                );
                            */
                        }
                    }
                }
                return cmnts;
            }
            return new List<DocumentationComment>();
        }
    }
}
