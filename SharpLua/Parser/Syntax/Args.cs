using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class Args
    {
        public List<Expr> ArgList = new List<Expr>();

        public StringLiteral String;

        public TableConstructor Table;

    }
}
