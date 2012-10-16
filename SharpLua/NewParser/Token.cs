using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class Token
    {
        public TokenType Type;
        /// <summary>
        /// Leading whitespace and comments
        /// </summary>
        public List<Token> Leading = new List<Token>();
        /// <summary>
        /// Only value will be the EndOfStream token.
        /// </summary>
        public Token FollowingEoSToken = null;
        public string Data;
        public int Line, Column;

        public Token()
        {
            Line = 0;
            Column = 0;
            Data = "";
            Type = TokenType.UNKNOWN;
        }

        public string Print()
        {
            return "<" + Type.ToString() + ", Data='" + Data + "', LeadingCount = " + Leading.Count + ", Line/Col=" + Line + "/" + Column + ">";
        }
    }

    public enum TokenType
    {
        UNKNOWN = -1,

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
        Shebang,

        EndOfStream,
    }
}
