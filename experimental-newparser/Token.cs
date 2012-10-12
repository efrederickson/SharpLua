using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser
{
    public class Token
    {
        public TokenType Type;
        /// <summary>
        /// Leading whitespace and comments
        /// </summary>
        public List<Token> Leading;
        public string Data;
        public int Line, Column;

        public string Print()
        {
            return "<" + Type.ToString() + ", Data='" + Data + "', Line/Col=" + Line + "/" + Column + ">";
        }
    }

    public enum TokenType
    {
        Keyword,
        Ident,

        Number,
        SingleQuoteString,
        DoubleQuoteString,
        LongString,

        Symbol,

        WhitespaceSpace, // ' '
        WhitespaceTab,   // \t
        WhitespaceN,     // \n
        WhitespaceR,     // \r
        ShortComment,
        LongComment,

        EndOfStream,
    }
}
