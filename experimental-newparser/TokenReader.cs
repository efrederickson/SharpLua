using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser
{
    public class TokenReader
    {
        Stack<int> savedP = new Stack<int>();
        public int p = 0;
        public List<Token> tokens;

        public TokenReader(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        //getters
        public Token Peek(int n = 0)
        {
            return tokens[Math.Min(tokens.Count - 1, p + n)];
        }

        public Token Get()
        {
            Token t = tokens[p];
            p = Math.Min(p + 1, tokens.Count);
            return t;
        }

        public bool Is(TokenType t)
        {
            return Peek().Type == t;
        }

        //save / restore points in the stream
        public void Save()
        {
            savedP.Push(p);
        }

        public void Commit()
        {
            savedP.Pop();
        }

        public void Restore()
        {
            p = savedP.Pop();
        }

        //either return a symbol if there is one, or return true if the requested
        //symbol was gotten.
        public bool ConsumeSymbol(string symb)
        {
            Token t = Peek();
            if (t.Type == TokenType.Symbol)
            {
                if (t.Data == symb)
                {
                    Get();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool ConsumeSymbol(char sym)
        {
            Token t = Peek();
            if (t.Type == TokenType.Symbol)
            {
                if (t.Data == sym.ToString())
                {
                    Get();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;

        }

        public Token ConsumeSymbol()
        {
            Token t = Peek();
            if (t.Type == TokenType.Symbol)
            {
                Get();
                return t;
            }
            else
                return null;
        }

        public bool ConsumeKeyword(string kw)
        {
            Token t = Peek();
            if (t.Type == TokenType.Keyword && t.Data == kw)
            {
                Get();
                return true;
            }
            else
                return false;
        }

        public bool IsKeyword(string kw)
        {
            Token t = Peek();
            return t.Type == TokenType.Keyword && t.Data == kw;
        }

        public bool IsSymbol(string s)
        {
            Token t = Peek();
            return t.Type == TokenType.Symbol && t.Data == s;
        }

        public bool IsSymbol(char c)
        {
            Token t = Peek();
            return t.Type == TokenType.Symbol && t.Data == c.ToString();
        }

        public bool IsEof()
        {
            return Peek().Type == TokenType.EndOfStream;
        }

        public List<Token> Range(int start, int end)
        {
            List<Token> t = new List<Token>();
            for (int i = start; i < end; i++)
                t.Add(tokens[i]);
            return t;
        }
    }
}
