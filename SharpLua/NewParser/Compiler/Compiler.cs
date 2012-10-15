using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua;

namespace SharpLua.Compiler
{
    public class Compiler
    {
        Var2Reg v2r = new Var2Reg();
        Lua.Proto main = new Lua.Proto();
        Lua.Proto current = null;

        public Compiler() { current = main; }

        void DoExpr(Expression e)
        {
            if (e is AnonymousFunctionExpr) // function() ... end
                ;
            else if (e is BinOpExpr)
                ;
            else if (e is BoolExpr)
                ;
            else if (e is CallExpr && (!(e is StringCallExpr) && !(e is TableCallExpr)))
                ;
            else if (e is StringCallExpr)
                ;
            else if (e is TableCallExpr)
                ;
            else if (e is IndexExpr)
                ;
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
                ;
            else if (e is TableConstructorKeyExpr)
                ;
            else if (e is MemberExpr)
                ;
            else if (e is NilExpr)
                ;
            else if (e is NumberExpr)
                ;
            else if (e is StringExpr)
                ;
            else if (e is TableConstructorStringKeyExpr)
                ;
            else if (e is TableConstructorExpr)
                ;
            else if (e is UnOpExpr)
                ;
            else if (e is TableConstructorValueExpr)
                ;
            else if (e is VarargExpr)
                ;
            else if (e is VariableExpression)
                ;

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }

        void DoStatement(Statement s)
        {
            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                if (a.IsLocal)
                {

                }
                else
                {

                }
            }
            else if (s is AugmentedAssignmentStatement)
                ;
            else if (s is BreakStatement)
                ;
            else if (s is CallStatement)
                ;
            else if (s is DoStatement)
                ;
            else if (s is GenericForStatement)
                ;
            else if (s is NumericForStatement)
                ;
            else if (s is FunctionStatement)
                ;
            else if (s is GotoStatement)
                ;
            else if (s is IfStmt)
                ;
            else if (s is LabelStatement)
                ;
            else if (s is RepeatStatement)
                ;
            else if (s is ReturnStatement)
                ;
            else if (s is UsingStatement)
                ;
            else if (s is WhileStatement)
                ;

            else if (s is ElseIfStmt)
                ;
            else if (s is ElseStmt)
                ;

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        void DoChunk(Chunk c)
        {
            foreach (Statement s in c.Body)
                DoStatement(s);
        }

        public Lua.Proto Compile(Chunk c)
        {
            DoChunk(c);
            return main;
        }

        uint loadk(int reg, int k)
        {
            uint r = 0;
            Lua.SET_OPCODE(ref r, Lua.OpCode.OP_LOADK);
            return r;
        }
    }
}
