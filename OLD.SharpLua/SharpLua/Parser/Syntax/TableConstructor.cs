using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class TableConstructor : Term
    {
        public List<Field> FieldList = new List<Field>();

    }
}
