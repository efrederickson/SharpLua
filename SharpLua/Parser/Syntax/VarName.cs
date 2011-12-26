using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    public partial class VarName : BaseExpr
    {
        /// <summary>
        /// The name of the variable
        /// </summary>
        public string Name;
    
    }
}
