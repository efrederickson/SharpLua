using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    public abstract class Statement
    {
        public Scope Scope = null;
        public bool HasSemicolon = false;
        public int LineNumber = 0;

        public List<Token> ScannedTokens = new List<Token>();
        public Token SemicolonToken;

        public virtual Statement Simplify()
        {
            return this;
        }
    }

}
