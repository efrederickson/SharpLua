using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class KeyValue : Field
    {
        public Expr Key;

    }
}
