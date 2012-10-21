using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;

namespace SharpLua
{
    public partial class Refactoring
    {
        public static bool CanInline(AnonymousFunctionExpr e)
        {
            if (e.Body.Count > 0 && e.Body[0] is ReturnStatement)
                return true;
            return false;
        }

        public static InlineFunctionExpression InlineFunction(AnonymousFunctionExpr e)
        {
            if (!CanInline(e))
                throw new Exception("Cannot inline function!");

            ReturnStatement rs = e.Body[0] as ReturnStatement;

            InlineFunctionExpression ife = new InlineFunctionExpression();
            foreach (Variable v in e.Arguments)
                ife.Arguments.Add(v);
            ife.IsVararg = e.IsVararg;

            foreach (Expression expr in rs.Arguments)
                ife.Expressions.Add(expr);

            ife.Scope = e.Scope;
            rs.Scope = e.Scope;
            return ife;
        }
    }
}
