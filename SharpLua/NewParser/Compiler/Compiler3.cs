using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua;
using SharpLua.LASM;

namespace SharpLua.Compiler
{
    /*
     * TODO/Known bugs:
    
     * [B] Strings of length 0 are Constanted as nil...
     * [T] Presimplify binary operators
     * [T] Use constant fields in binary operators when possible (currently uses LOADK)
     * [T] BoolExpr 'C' arg
    */

    public class Compiler
    {
        LuaFile file;
        Block block;
        int line = 0;

        public Compiler() { }

        void DoExpr(Expression e, bool setVar = false, int setVarLhsCount = -1)
        {
            if (e is AnonymousFunctionExpr) // function() ... end
            {
            }
            else if (e is BinOpExpr)
            {
                BinOpExpr boe = e as BinOpExpr;
                switch (boe.GetOperator())
                {
                    case BinaryOperator.Add:
                        binOp("ADD", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Subtract:
                        binOp("SUB", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Multiply:
                        binOp("MUL", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Divide:
                        binOp("DIV", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Power:
                        binOp("POW", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Modulus:
                        binOp("MOD", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.Concat:
                        binOp("CONCAT", boe.Lhs, boe.Rhs);
                        return;
                    case BinaryOperator.And:
                        break;
                    case BinaryOperator.Or:
                        break;
                    case BinaryOperator.LessThan:
                        break;
                    case BinaryOperator.LessThanOrEqualTo:
                        break;
                    case BinaryOperator.GreaterThan:
                        break;
                    case BinaryOperator.GreaterThanOrEqualTo:
                        break;
                    case BinaryOperator.NotEqual:
                        break;
                    case BinaryOperator.ShiftRight:
                        CallExpr ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "rshift",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.ShiftLeft:
                        ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "lshift",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.Xor:
                        ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "bxor",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.BitAnd:
                        ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "band",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.BitOr:
                        ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "bor",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.BitNot:
                        ce = new CallExpr();
                        ce.Arguments.Add(boe.Lhs);
                        ce.Arguments.Add(boe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "bnot",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case BinaryOperator.NONE:
                    default:
                        throw new Exception("Unknown binary operator '" + boe.Op + "'");
                }
            }
            else if (e is BoolExpr)
            {
                bool v = ((BoolExpr)e).Value;
                Instruction i = new Instruction("LOADBOOL");
                i.A = block.getreg();
                i.B = v ? 1 : 0;
                i.C = 0;
                emit(i);
                return;
            }
            else if (e is CallExpr)//&& (!(e is StringCallExpr) && !(e is TableCallExpr)))
            {
                CallExpr ce = e as CallExpr;
                int breg = block.regnum;
                DoExpr(ce.Base);
                bool isZero = false;
                foreach (Expression e2 in ce.Arguments)
                {
                    DoExpr(e2);
                    if (e2 is CallExpr)
                        isZero = true;
                }

                Instruction i = new Instruction("CALL");
                i.A = breg;
                i.B = isZero ? 0 : (ce.Arguments.Count > 0 ? 1 + ce.Arguments.Count : 1);
                i.C = setVarLhsCount == 0 || setVarLhsCount == -1 ? 1 : 1 + setVarLhsCount;
                emit(i);
                return;
            }
            else if (e is StringCallExpr)
            {
            }
            else if (e is TableCallExpr)
            {
            }
            else if (e is IndexExpr)
            {
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
            }
            else if (e is MemberExpr)
            {
            }
            else if (e is NilExpr)
            {
                Instruction i = new Instruction("LOADNIL");
                i.A = block.getreg();
                i.B = setVarLhsCount == -1 ? i.A : setVarLhsCount - 1;
                i.C = 0;
                emit(i);
                return;
            }
            else if (e is NumberExpr)
            {
                NumberExpr ne = e as NumberExpr;

                // TODO: this can optimized into a Dictionary to avoid re-parsing numbers each time
                double r;
                int x = Lua.luaO_str2d(ne.Value, out r);
                if (x == 0)
                    throw new LuaSourceException(line, 0, "Invalid number");

                //block.K.Check(r);

                Instruction i = new Instruction("loadk");
                i.A = block.getreg();
                i.Bx = block.K[r];
                emit(i);
                return;
            }
            else if (e is StringExpr)
            {

                StringExpr se = e as StringExpr;
                string s = se.Value;
                if (se.StringType != TokenType.LongString)
                    s = Unescaper.Unescape(s);
                else
                {
                    int i = 1;
                    while (s[i] != '[')
                        i++;
                    i++;
                    s = s.Substring(i, s.Length - i - 2);
                }

                //block.K.Check(s);
                Instruction i2 = new Instruction("loadk");
                i2.A = block.getreg();
                i2.Bx = block.K[s];
                emit(i2);
                return;
            }
            else if (e is TableConstructorExpr)
            {
            }
            else if (e is UnOpExpr)
            {
            }
            else if (e is VarargExpr)
            {
            }
            else if (e is VariableExpression)
            {
                VariableExpression ve = e as VariableExpression;
                if (block.V.has(ve.Var.Name) && ve.Var.IsGlobal == false)
                { // local
                    Instruction i = new Instruction("move");
                    i.B = block.V[ve.Var.Name]; // moved into here
                    i.A = block.getreg(); // from here
                    emit(i);

                    block.CheckLocalName(ve.Var.Name);
                    //// TODO: Emit local name into block.Chunk.Locals
                }
                else
                { // global
                    Instruction i = null;
                    if (setVar)
                    {
                        i = new Instruction("setglobal");
                        i.A = block.regnum - 1; // ret
                    }
                    else
                    {
                        i = new Instruction("getglobal");
                        i.A = block.getreg(); // ret
                    }

                    i.Bx = block.K[ve.Var.Name]; // const
                    emit(i);
                }
                return;
            }

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }

        void DoStatement(Statement s)
        {
            line = s.LineNumber;
            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                if (a.IsLocal == false)
                {

                    for (int i = a.Rhs.Count; i > 0; i--)
                        DoExpr(a.Rhs[i - 1], false, a.Lhs.Count);

                    for (int i = a.Lhs.Count; i > 0; i--)
                        DoExpr(a.Lhs[i - 1], true);

                    return;
                }
            }
            else if (s is AugmentedAssignmentStatement)
                ;
            else if (s is BreakStatement)
            {
            }
            else if (s is ContinueStatement)
            {
            }
            else if (s is CallStatement)
            {
                CallStatement cs = s as CallStatement;
                DoExpr(cs.Expression);
                return;
            }
            else if (s is DoStatement)
            {
            }
            else if (s is GenericForStatement)
            {
            }
            else if (s is NumericForStatement)
            {
            }
            else if (s is FunctionStatement)
            {

            }
            else if (s is GotoStatement)
                ;
            else if (s is IfStmt)
            {
            }
            else if (s is LabelStatement)
                ;
            else if (s is RepeatStatement)
            {
            }
            else if (s is ReturnStatement)
            {
            }
            else if (s is UsingStatement)
            {
            }
            else if (s is WhileStatement)
            {
            }

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        void DoChunk(Ast.Chunk c)
        {
            DoChunk(c.Body);
        }

        void DoChunk(List<Statement> ss)
        {
            foreach (Statement s in ss)
            {
                DoStatement(s);
                block.regnum = 0;
            }
        }

        public LuaFile Compile(Ast.Chunk c, string name)
        {
            file = new LuaFile();
            block = new Block();
            block.Chunk.Name = name;

            DoChunk(c);

            file.Main = block.Chunk;
            file.Main.ArgumentCount = 0;
            file.Main.Vararg = 2;
            file.Main.UpvalueCount = file.Main.Upvalues.Count;
            bool addRet = file.Main.Instructions.Count == 0;
            if (addRet == false)
                addRet = file.Main.Instructions[file.Main.Instructions.Count - 1].Opcode != Instruction.LuaOpcode.RETURN;
            if (addRet)
            {
                Instruction ret = new Instruction("RETURN");
                ret.A = 0;
                ret.B = 1;
                ret.C = 0;
                file.Main.Instructions.Add(ret);
            }
            return file;
        }

        void emit(Instruction i)
        {
            block.Chunk.Instructions.Add(i);
        }

        void binOp(string opname, Expression left, Expression right)
        {
            // this and the variable reg2 are saved so that we can use the locations in
            // args B and C to the opcode
            int reg1 = block.regnum;
            DoExpr(left);
            int reg2 = block.regnum;
            DoExpr(right);
            Instruction i = new Instruction(opname);

            // We no longer need reg1 through the top, so we can overwrite them
            block.regnum = reg1;

            i.A = block.getreg();

            // >255 is const

            i.B = reg1;
            i.C = reg2;
            emit(i);
        }
    }
}
