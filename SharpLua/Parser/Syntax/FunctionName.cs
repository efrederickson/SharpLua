using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class FunctionName
    {
        public List<string> FullName = new List<string>();

        public string MethodName;

    }
}
