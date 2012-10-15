using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast.Statement
{
    /// <summary>
    /// This way we know that the first BinOpExpr's Op is used in the assignment.
    /// </summary>
    public class AugmentedAssignmentStatement : AssignmentStatement
    {
    }
}
