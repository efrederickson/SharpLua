using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Statement;
using SharpLua.Ast.Expression;

namespace SharpLua
{
    public partial class Refactoring
    {
        // TODO: Check FunctionStatement Name's also

        public static Location FindImplementation(Chunk c, Variable v)
        {
            Location ret = null;
            foreach (Statement s in c.Body)
            {
                if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement)) // couldn't be defined in AugmentedAssignment
                {
                    AssignmentStatement a = s as AssignmentStatement;
                    int tokPos = 0;
                    if (a.IsLocal)
                        tokPos++;
                    foreach (Expression e in a.Lhs)
                    {
                        int t = tokPos;
                        tokPos++;
                        if (e is VariableExpression)
                        {
                            VariableExpression var = e as VariableExpression;
                            if (var.Var == v)
                            {
                                if (a.ScannedTokens.Count >= t && a.ScannedTokens[t].Type == TokenType.Ident)
                                {
                                    Token tok = a.ScannedTokens[t];
                                    ret = tok.Location;
                                    break;
                                }
                            }
                        }
                        tokPos++;
                    }
                }
                else if (s is FunctionStatement)
                {
                    FunctionStatement fs = s as FunctionStatement;
                    Variable var = null;
                    int p = 1;
                    if (fs.IsLocal)
                        p++;
                    Expression e = fs.Name;
                    while (e is IndexExpr)
                    {
                        e = ((IndexExpr)e).Index;
                        p += 2; // <varname> '.'
                    }
                    if (e is VariableExpression)
                        var = ((VariableExpression)e).Var;

                    if (var != null)
                    {
                        if (var == v)
                        {
                            if (fs.ScannedTokens.Count >= p && fs.ScannedTokens[p].Type == TokenType.Ident)
                            {
                                Token tok = fs.ScannedTokens[p];
                                ret = tok.Location;
                                break;
                            }
                        }
                    }
                }

                if (s is Chunk && ret == null)
                {
                    ret = FindImplementation(s as Chunk, v);
                }
            }
            return ret == null ? new Location() { Line = -1, Column = -1 } : ret;
        }
    }
}
