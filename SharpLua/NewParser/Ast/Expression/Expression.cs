using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Expression
{
    public abstract class Expression
    {
        public List<Token> LeadingWhitespaceAndComments = new List<Token>();
        public int ParenCount = 0;
        public Scope Scope = null;
    }
}
