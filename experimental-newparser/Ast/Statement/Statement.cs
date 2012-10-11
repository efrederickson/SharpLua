using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser.Ast.Statement
{
    public abstract class Statement
    {
        public List<Token> LeadingWhitespaceAndComments = new List<Token>();
    }
}
