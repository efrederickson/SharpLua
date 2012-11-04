using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class Lexer
    {
        string src = "";

        int p = 0;
        int ln = 1;
        int col = 1;

        char peek(int n = 0)
        {
            if (src.Length < p + n + 1)
                return '\0';
            else
                return src.Substring(p + n, 1)[0];
        }

        char read()
        {
            if (src.Length < p + 1)
                return '\0';
            else
            {
                char c = src.Substring(p, 1)[0];
                if (c == '\n')
                {
                    ln++;
                    col = 0; // incremented 2 lines down
                }
                col++;
                p++;
                return c;
            }
        }

        static bool IsSymbol(char c)
        {
            foreach (char c2 in (new char[] { '+', '-', '*', '/', '^', '%', ',', 
                '{', '}', '[', ']', '(', ')', ';', '#',
            '|', '&', }))
                if (c == c2)
                    return true;
            return false;
        }

        static bool IsHexDigit(char c)
        {
            return char.IsDigit(c) ||
                c == 'A' ||
                c == 'a' ||
                c == 'B' ||
                c == 'b' ||
                c == 'C' ||
                c == 'c' ||
                c == 'D' ||
                c == 'd' ||
                c == 'E' ||
                c == 'e' ||
                c == 'F' ||
                c == 'f';
        }

        static bool IsKeyword(string word)
        {
            return
                word == "and" ||
                word == "break" ||
                word == "do" ||
                word == "else" ||
                word == "elseif" ||
                word == "end" ||
                word == "false" ||
                word == "for" ||
                word == "function" ||
#if !VANILLA_LUA
 word == "goto" ||
#endif
 word == "if" ||
                word == "in" ||
                word == "local" ||
                word == "nil" ||
                word == "not" ||
                word == "or" ||
                word == "repeat" ||
                word == "return" ||
                word == "then" ||
                word == "true" ||
                word == "until" ||
                word == "while"
#if !VANILLA_LUA
 || word == "using"
                || word == "continue";
#else
;
#endif
        }

        string readnl()
        {
            if (peek() == '\r')
            {
                read();
                read();
                return "\r\n";
            }
            else
            {
                read();
                return "\n";
            }
        }

        // null = no long string
        string tryReadLongStr()
        {
            // first one was eaten
            //if (peek() != '[')
            //    return null;
            //read(); // read first '['

            int depth = 1;
            int numEquals = 0;
            while (peek() == '=')
            {
                numEquals++;
                read();
            }
            if (peek() != '[')
                return null;
            read(); // read closing '['
            int start = p;
            while (true)
            {
                // check for eof
                if (peek() == '\0')
                    error("Expected ']" + "=".Repeat(numEquals) + "]' near <eof>");


                // check for the end
                bool foundEnd = true;
                if (peek() == ']')
                {
                    for (int i = 0; i < numEquals; i++)
                        if (peek(i + 1) != '=')
                            foundEnd = false;
                    if (peek(numEquals + 1) != ']')
                        foundEnd = false;
                }
                else
                {
                    if (peek() == '[')
                    {
                        // is there an embedded long string?
                        bool embedded = true;
                        for (int i = 0; i < numEquals; i++)
                        {
                            if (peek(i + 1) != '=')
                            {
                                embedded = false;
                                break;
                            }
                        }
                        if (peek(numEquals + 2) == '[' && embedded)
                        {
#if !VANILLA_LUA
                            // oh look, there was
                            depth++;
                            for (int i = 0; i < numEquals + 2; i++)
                                read();
#else
                            error("Embedded long strings is deprecated");
#endif
                        }
                    }
                    foundEnd = false;
                }
                if (foundEnd)
                {
                    depth--;
                    if (depth == 0)
                        break;
                    else
                    {
                        for (int i = 0; i < numEquals + 2; i++)
                            read();
                    }
                }
                else
                {
                    read();
                }
            }
            string content = src.Substring(start, p - start);
            for (int i = 0; i < numEquals + 2; i++) // read closing ](=*)]
                if (peek() == '=' || peek() == ']')
                    read();
                else // wat
                    break;
            return "[" + "=".Repeat(numEquals) + "[" + content + "]" + "=".Repeat(numEquals) + "]";
        }

        void error(string msg)
        {
            throw new LuaSourceException(ln, col, msg);
        }

        bool matchpeek(string chars)
        {
            char c = peek();
            foreach (char c2 in chars)
                if (c == c2)
                    return true;
            return false;
        }

        public TokenReader Lex(string s)
        {
            List<Token> tokens = new List<Token>();
            src = s;
            p = 0;
            ln = 1;
            col = 1;

            while (true)
            {
                List<Token> leading = new List<Token>();
                // eat whitespace/comments
                while (true)
                {
                    char c_ = peek();
                    if (c_ == '#' && peek(1) == '!' && ln == 1)
                    {
                        // linux shebang
                        string sh = "";
                        while (peek() != '\n' && peek() != '\r' && peek() != '\0')
                        {
                            sh += read();
                        }
                        //readnl();
                        leading.Add(new Token { Type = TokenType.Shebang, Data = sh });
                    }
                    else if (c_ == ' ' || c_ == '\t')
                    {
                        leading.Add(new Token { Type = c_ == ' ' ? TokenType.WhitespaceSpace : TokenType.WhitespaceTab, Data = c_.ToString() });
                        read();
                    }
                    else if (c_ == '-' && peek(1) == '-')
                    {
                        read();
                        read();
                        char openParen = '\0';
                        if (peek() == '[')
                            openParen = read();
                        string comment = tryReadLongStr();
                        if (comment == null)
                        {
                            comment = "--" + (openParen != '\0' ? openParen.ToString() : "");
                            while (peek() != '\n' && peek() != '\r' && peek() != '\0')
                                comment += read();
                            //read();
                            //if (peek() == '\n') // e.g. \r\n
                            //    read();
                            if (comment.Length >= 3 && comment.Substring(0, 3) == "---")
                                leading.Add(new Token { Type = TokenType.DocumentationComment, Data = comment });
                            else
                                leading.Add(new Token { Type = TokenType.ShortComment, Data = comment });
                        }
                        else
                        {
                            comment = "--" + comment;
                            leading.Add(new Token { Type = TokenType.LongComment, Data = comment });
                        }
                    }
                    else if (c_ == '\n' || c_ == '\r')
                    {
                        leading.Add(new Token { Type = c_ == '\n' ? TokenType.WhitespaceN : TokenType.WhitespaceR, Data = c_.ToString() });
                        // read handles line changing...
                        read();
                    }
                    else
                        break;
                }

                Token t = new Token();
                t.Leading = leading;
                t.Line = ln;
                t.Column = col;

                char c = read();

                if (c == '\0')
                    t.Type = TokenType.EndOfStream;
                else if (char.IsLetter(c) || c == '_')
                {
                    // ident / keyword
                    string s4 = c.ToString();
                    while (char.IsLetter(peek()) ||
                        peek() == '_' ||
                        char.IsDigit(peek()) &&
                        peek() != '\0')
                    {
                        s4 += read();
                    }
                    t.Data = s4;
                    if (IsKeyword(s4))
                        t.Type = TokenType.Keyword;
                    else
                        t.Type = TokenType.Ident;
                }
                else if (char.IsDigit(c) ||
                    (c == '.' && char.IsDigit(peek())))
                { // negative numbers are handled in unary minus collection
                    string num = "";
                    if (c == '0' && matchpeek("xX"))
                    {
                        //read(); -> already done
                        num = "0" + read(); // 'xX'
                        while (IsHexDigit(peek()))
                            num += read();
                    }
#if !VANILLA_LUA
                    else if (c == '0' && matchpeek("bB"))
                    {
                        num = "0" + read(); // read 'bB'
                        while (char.IsDigit(peek()) || peek() == '_')
                            num += read();
                    }
                    else if (c == '0' && matchpeek("oO"))
                    {
                        num = "0" + read(); // read 'oO'
                        while (char.IsDigit(peek()) || peek() == '_')
                            num += read();
                    }
#endif
                    else
                    {
                        num = c.ToString();
                        bool dec = false;
                        while (char.IsDigit(peek())
                            || peek() == '.'
#if !VANILLA_LUA
 || peek() == '_')
#else
                            )
#endif
                        {
                            num += read();
                            if (peek() == '.')
                            {
                                if (dec)
                                    error("Number has more than one decimal point");
                                dec = true;
                                num += read();
                            }
                        }
                    }
#if !VANILLA_LUA
                    if (matchpeek("PpEe")) // exponent
                    {
                        num += read();
                        if (matchpeek("+-"))
                            num += read();
                        while (char.IsDigit(peek()))
                            num += read();
                    }
#endif
                    t.Data = num;
                    t.Type = TokenType.Number;
                }
                else if (c == '\'' || c == '"')
                {
                    char delim = c;
                    string str = "";
                    while (true)
                    {
                        char c2 = read();
                        if (c2 == '\\')
                        {
                            str += "\\";
                            str += read(); // we won't parse \0xFF, \000, \n, etc here
                        }
                        else if (c2 == delim)
                            break;
                        else if (c2 == '\0')
                            error("expected '" + delim + "', not <eof>");
                        else
                            str += c2;
                    }
                    t.Data = str;
                    t.Type = delim == '"' ? TokenType.DoubleQuoteString : TokenType.SingleQuoteString;
                }
                else if (c == '[')
                {
                    string s3 = tryReadLongStr();
                    if (s3 == null)
                    {
                        t.Type = TokenType.Symbol;
                        t.Data = "[";
                    }
                    else
                    {
                        t.Type = TokenType.LongString;
                        t.Data = s3;
                    }
                }
                else if (c == '<' || c == '>' || c == '=')
                {
                    t.Type = TokenType.Symbol;
                    if (peek() == '=' ||
                        (c == '<' && peek() == '<') ||
                        (c == '>' && peek() == '>'))
                    {
                        t.Data = c.ToString() + read().ToString();
#if !VANILLA_LUA
                        if (peek() == '=' && (c == '<' || c == '>'))
                            t.Data += read(); // augmented, e.g. >>=, but not ===
#endif
                    }
                    else
                        t.Data = c.ToString();
                }
                else if (c == '~')
                {
                    if (peek() == '=')
                    {
                        read();
                        t.Type = TokenType.Symbol;
                        t.Data = "~=";
                    }
                    else
                    {
                        t.Type = TokenType.Symbol;
                        t.Data = "~";
                    }

                }
                else if (c == '.')
                {
                    t.Type = TokenType.Symbol;
                    if (peek() == '.')
                    {
                        read(); // read second '.
                        if (peek() == '.')
                        {
                            t.Data = "...";
                            read(); // read third '.'
                        }
#if !VANILLA_LUA
                        else if (peek() == '=') // ..=
                        {
                            t.Data = "..=";
                            read(); // read '='
                        }
#endif
                        else
                            t.Data = "..";
                    }
                    else
                    {
                        t.Data = ".";
                    }
                }
                else if (c == ':')
                {
                    t.Type = TokenType.Symbol;
#if !VANILLA_LUA
                    if (peek() == ':')
                    {
                        read();
                        t.Data = "::";
                    }
                    else
#endif
                        t.Data = ":";
                }
                else if (c == '-' && peek() == '>')
                {
                    read(); // read the '>'
                    t.Data = "->";
                    t.Type = TokenType.Symbol;
                }
                else if (c == '^')
                {
                    t.Type = TokenType.Symbol;

                    if (peek() == '^')
                    {
                        read();
#if !VANILLA_LUA
                        if (peek() == '=')
                        {
                            read();
                            t.Data = "^^=";
                        }
                        else
#endif
                            t.Data = "^^";
                    }
#if !VANILLA_LUA
                    else if (peek() == '=')
                    {
                        read();
                        t.Data = "^=";
                    }
#endif
                    else
                        t.Data = "^";
                }
#if !VANILLA_LUA
                else if (c == '!')
                {
                    t.Type = TokenType.Symbol;
                    if (peek() == '=')
                    {
                        read();
                        t.Data = "!=";
                    }
                    else t.Data = "!";
                }
#endif
                else if (IsSymbol(c))
                {
                    t.Type = TokenType.Symbol;
                    t.Data = c.ToString();
#if !VANILLA_LUA
                    if (peek() == '=')
                    {
                        char c2 = peek();
                        if (c == '+' ||
                            c == '-' ||
                            c == '/' ||
                            c == '*' ||
                            c == '^' ||
                            c == '%' ||
                            c == '&' ||
                            c == '|')
                        {
                            t.Data += "=";
                            read();
                        }
                    }
#endif
                }
                else
                {
                    p--; // un-read token
                    col--;
                    error("Unexpected Symbol '" + c + "'");
                    read();
                }

                tokens.Add(t);

                if (peek() == '\0')
                    break;
            }
            if (tokens.Count > 0 && tokens[tokens.Count - 1].Type != TokenType.EndOfStream)
                tokens.Add(new Token { Type = TokenType.EndOfStream });
            if (tokens.Count > 1) // 2+
                tokens[tokens.Count - 2].FollowingEoSToken = tokens[tokens.Count - 1];
            return new TokenReader(tokens);
        }
    }
}
