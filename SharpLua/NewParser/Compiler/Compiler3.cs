using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua;
using SharpLua.LASM;
using System.Diagnostics;

namespace SharpLua.Compiler
{
    /*
     * TODO/Known bugs:
    
     * [T] Presimplify binary operators
     * [T] Use constant fields in binary operators when possible (currently uses LOADK)
     * [T] BoolExpr 'C' arg
     * [B] Vararg is all messed up...
     * [T] Vararg needs work for something like return ..., 1
     * [T] TableConstructorNamedFunctionExpr
     * [B] Tables won't compile...
     * [T] Somehow, I should be able to make augmented assignments only look up Lhs var once...
    */

    public class Compiler
    {
        LuaFile file;
        Block block;
        int line = 0;
        Stack<Instruction> patchSbx = new Stack<Instruction>();


        public Compiler() { }

        void DoExpr(Expression e, bool setVar = false, int setVarLhsCount = -1, bool onlyCheckConsts = false)
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
                int breg = ++block.regnum;
                DoExpr(ce.Base);
                bool isZero = false;
                bool isMethod = false;
                Expression ex = ce.Base;
                while (ex != null)
                {
                    if (ex is IndexExpr)
                        ex = ((IndexExpr)ex).Index;
                    else if (ex is MemberExpr)
                    {
                        MemberExpr me = ex as MemberExpr;
                        if (me.Indexer == ":")
                        {
                            isMethod = true;
                            break;
                        }
                        else
                            break;
                        //ex = me.Ident;
                    }
                    else
                        break;
                }

                foreach (Expression e2 in ce.Arguments)
                {
                    DoExpr(e2);
                    if (e2 is CallExpr || block.Chunk.Instructions[block.Chunk.Instructions.Count - 1].Opcode == Instruction.LuaOpcode.CALL)
                    {
                        isZero = true;
                        Instruction i_ = block.Chunk.Instructions[block.Chunk.Instructions.Count - 1];
                        Debug.Assert(i_.Opcode == Instruction.LuaOpcode.CALL);
                        i_.C = 0;
                    }
                }

                Instruction i = new Instruction("CALL");
                i.A = breg;
                if (isMethod)
                    //i.B++;
                    i.B = isZero ? 2 : (ce.Arguments.Count > 0 ? 2 + ce.Arguments.Count : 2);
                else
                    i.B = isZero ? 0 : (ce.Arguments.Count > 0 ? 1 + ce.Arguments.Count : 1);

                i.C = setVarLhsCount == 0 || setVarLhsCount == -1 ?
                    1 : //(isZero ? 0 : 1) :
                    1 + setVarLhsCount; // (isZero ? 0 : 1 + setVarLhsCount);
                //i.C = setVarLhsCount == 0 || setVarLhsCount == -1 ? 1 : 1 + setVarLhsCount;
                emit(i);
                return;
            }
            else if (e is StringCallExpr)
            {
                throw new Exception();
            }
            else if (e is TableCallExpr)
            {
                throw new Exception();
            }
            else if (e is IndexExpr)
            {
                IndexExpr ie = e as IndexExpr;
                int regnum = block.regnum;
                DoExpr(ie.Base);
                DoExpr(ie.Index);
                Instruction i = new Instruction("GETTABLE");
                i.A = regnum;
                i.B = regnum;
                i.C = block.regnum - 1;// block.getreg();
                emit(i);
                block.regnum = regnum + 1;
                return;
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
                InlineFunctionExpression i = e as InlineFunctionExpression;
                AnonymousFunctionExpr af = new AnonymousFunctionExpr();
                af.Arguments = i.Arguments;
                af.IsVararg = i.IsVararg;
                af.Body = new List<Statement>()
                {
                    new ReturnStatement
                    {
                        Arguments = i.Expressions
                    }
                };
                DoExpr(af);
            }
            else if (e is MemberExpr)
            {
                MemberExpr me = e as MemberExpr;
                if (me.Indexer == ".")
                {
                    int regnum = block.regnum;
                    DoExpr(me.Base);
                    DoExpr(new StringExpr(me.Ident), false, -1, true);
                    Instruction i = new Instruction("GETTABLE");
                    i.A = regnum;
                    i.B = regnum;
                    i.C = 256 + block.K[me.Ident];
                    //i.C = block.regnum - 1;// block.getreg();
                    emit(i);
                    block.regnum = regnum + 1;
                    return;
                }
                else if (me.Indexer == ":")
                {
                    int regnum = block.regnum;
                    DoExpr(me.Base);
                    DoExpr(new StringExpr(me.Ident), false, -1, true);
                    Instruction i = new Instruction("SELF");
                    i.A = regnum;
                    i.B = regnum;
                    i.C = 256 + block.K[me.Ident];
                    //i.C = block.regnum - 1;// block.getreg();
                    emit(i);
                    block.regnum = regnum + 1;
                    return;

                }
                else
                    throw new Exception("Unknown member indexer '" + me.Indexer + "'");
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
                int x = Lua.luaO_str2d(ne.Value.Replace("_", ""), out r);
                if (x == 0)
                    throw new LuaSourceException(line, 0, "Invalid number");

                if (onlyCheckConsts == false)
                {
                    Instruction i = new Instruction("loadk");
                    i.A = block.getreg();
                    i.Bx = block.K[r];
                    emit(i);
                }
                else
                    block.K.Check(r);
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

                if (onlyCheckConsts == false)
                {
                    Instruction i2 = new Instruction("loadk");
                    i2.A = block.getreg();
                    i2.Bx = block.K[s];
                    emit(i2);
                }
                else
                    block.K.Check(s);
                return;
            }
            else if (e is TableConstructorExpr)
            {
                Instruction i = new Instruction("NEWTABLE");
                int tblA = block.regnum;
                i.A = block.getreg();
                i.B = 0;
                i.C = 0;
                emit(i);

                TableConstructorExpr tce = e as TableConstructorExpr;
                if (tce.EntryList.Count == 0)
                    return;

                int b = 0;
                bool wasLastCall = false;
                foreach (Expression e2 in tce.EntryList)
                {
                    if (e2 is TableConstructorKeyExpr)
                    {
                        TableConstructorKeyExpr tcke = e2 as TableConstructorKeyExpr;
                        DoExpr(tcke.Key);
                        DoExpr(tcke.Value);
                    }
                    else if (e2 is TableConstructorNamedFunctionExpr)
                    {
                        TableConstructorNamedFunctionExpr tcnfe = e2 as TableConstructorNamedFunctionExpr;
                    }
                    else if (e2 is TableConstructorStringKeyExpr)
                    {
                        TableConstructorStringKeyExpr tcske = e2 as TableConstructorStringKeyExpr;
                        DoExpr(new StringExpr(tcske.Key));
                        DoExpr(tcske.Value);
                    }
                    else if (e2 is TableConstructorValueExpr)
                    {
                        TableConstructorValueExpr tcve = e2 as TableConstructorValueExpr;
                        DoExpr(tcve.Value);
                        if (tcve.Value is VarargExpr || tcve.Value is CallExpr)
                            wasLastCall = true;
                        else
                            wasLastCall = false;
                    }

                    b++;
                }

                i.B = b;

                i = new Instruction("SETLIST");
                i.A = tblA;
                if (wasLastCall)
                    i.B = 0;
                else
                    i.B = block.regnum - 1;
                i.C = tblA + 1;
                emit(i);
                block.regnum = tblA;
                return;
            }
            else if (e is UnOpExpr)
            {
                UnOpExpr uoe = e as UnOpExpr;
                switch (uoe.GetOperator())
                {
                    case UnaryOperator.Not:
                        unOp("NOT", uoe.Rhs);
                        return;
                    case UnaryOperator.Length:
                        unOp("LEN", uoe.Rhs);
                        return;
                    case UnaryOperator.BitNot:
                        CallExpr ce = new CallExpr();
                        ce.Arguments.Add(uoe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "bit", IsGlobal = true } },
                            Ident = "bnot",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case UnaryOperator.Negate:
                        unOp("UNM", uoe.Rhs);
                        return;
                    case UnaryOperator.UnNegate:
                        ce = new CallExpr();
                        ce.Arguments.Add(uoe.Rhs);
                        ce.Base = new MemberExpr()
                        {
                            Base = new VariableExpression() { Var = new Variable() { Name = "math", IsGlobal = true } },
                            Ident = "abs",
                            Indexer = ".",
                        };
                        DoExpr(ce);
                        return;
                    case UnaryOperator.NONE:
                    default:
                        throw new Exception("Unknown unary operator '" + uoe.Op + "'");
                }
            }
            else if (e is VarargExpr)
            {
                if (block.Chunk.Vararg == 0)
                    throw new LuaSourceException(0, 0, "Cannot use varargs (...) outside of a vararg function");
                Instruction i = new Instruction("VARARG");
                i.A = block.getreg();
                if (setVar)
                {
                    i.B = setVarLhsCount == -1 ? 0 : 1 + setVarLhsCount;
                }
                else
                {
                    i.B = 0;
                }
                emit(i);
                return;
            }
            else if (e is VariableExpression)
            {
                VariableExpression ve = e as VariableExpression;
                if (ve.Var.IsGlobal == false)
                { // local
                    if (setVar)
                    {
                        //Instruction i = new Instruction("move");
                        //i.B = block.V[ve.Var.Name]; // moved into here
                        //i.A = block.getreg(); // from here
                        //emit(i);

                        int _TMP_001_ = block.V[ve.Var.Name]; // Should probably just add a Check method in Var2Reg
                        block.CheckLocalName(ve.Var.Name);
                    }
                    else
                    {
                        Instruction i = new Instruction("move");
                        i.B = block.V[ve.Var.Name]; // moved into here
                        i.A = block.getreg(); // from here
                        emit(i);
                    }
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
                else
                {
                    for (int i = a.Rhs.Count; i > 0; i--)
                        DoExpr(a.Rhs[i - 1], false, a.Lhs.Count);

                    for (int i = a.Lhs.Count; i > 0; i--)
                        DoExpr(a.Lhs[i - 1], true);

                    return;
                }
            }
            else if (s is AugmentedAssignmentStatement)
            {
                AugmentedAssignmentStatement aas = s as AugmentedAssignmentStatement;
                StringBuilder sb = new StringBuilder();
                //sb.Append(DoExpr(a.Lhs[0]));
                if (aas.IsLocal)
                    throw new LuaSourceException(s.LineNumber, 0, "Local assignment cannot have augmented operators");

                DoExpr(aas.Rhs[0], true, 1);
                DoExpr(aas.Lhs[0], true, 1);
                return;
            }
            else if (s is BreakStatement)
            {
                bool hadLoop = false;
                while (block != null)
                {
                    if (block.IsLoop)
                    {
                        hadLoop = true;
                        break;
                    }
                    block = block.PreviousBlock;
                }
                if (!hadLoop)
                    throw new LuaSourceException(s.LineNumber, 0, "No loop to break");

                Instruction i = new Instruction("JMP");
                i.A = 0;
                i.sBx = -1; // Infinite loop ;)
                emit(i);
                patchSbx.Push(i);
                return;
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
                WhileStatement ws = s as WhileStatement;

                int start = block.Chunk.Instructions.Count;

                DoExpr(ws.Condition);

                Instruction t = new Instruction("TEST");
                t.A = block.regnum;
                t.C = 0;
                emit(t);

                DoChunk(ws.Body, true);

                Instruction i = new Instruction("JMP");
                i.A = 0;
                i.sBx = -(block.Chunk.Instructions.Count - start) - 1;
                emit(i);

                while (patchSbx.Count > 0)
                    patchSbx.Pop().sBx = Math.Abs(block.Chunk.Instructions.Count + i.sBx - 1);

                //return;
            }

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        void DoChunk(Ast.Chunk c, bool isloop = false)
        {
            DoChunk(c.Body, isloop);
        }

        void DoChunk(List<Statement> ss, bool isLoop = false)
        {
            block = new Block(block) { IsLoop = isLoop };
            foreach (Statement s in ss)
            {
                DoStatement(s);
                block.regnum = 0;
            }
            block = block.PreviousBlock;
        }

        public LuaFile Compile(Ast.Chunk c, string name)
        {
            file = new LuaFile();
            block = new Block();
            block.Chunk.Name = name;
            block.Chunk.ArgumentCount = 0;
            block.Chunk.Vararg = 2;

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

        double number(string s)
        {
            s = s.Replace("_", "");
            double ret;
            if (Lua.luaO_str2d(s, out ret) == 1)
                return ret;
            return 0;
        }

        bool constfolding(Expression a, Expression b, string opcode1, out double ret)
        {
            ret = 0;
            if (a is NumberExpr == false || b is NumberExpr == false)
                return false;
            double l = number(((NumberExpr)a).Value);
            double r = number(((NumberExpr)b).Value);
            Instruction opcode = new Instruction(opcode1);
            switch (opcode.Opcode)
            {
                case Instruction.LuaOpcode.ADD:
                    ret = l + r;
                    return true;
                case Instruction.LuaOpcode.SUB:
                    ret = l - r;
                    return true;
                    break;
                case Instruction.LuaOpcode.MUL:
                    ret = l * r;
                    return true;
                case Instruction.LuaOpcode.DIV:
                    if (l == 0 || r == 0)
                        return false;
                    ret = l / r;
                    return true;
                case Instruction.LuaOpcode.MOD:
                    if (l == 0 || r == 0)
                        return false;
                    ret = l % r;
                    return true;
                case Instruction.LuaOpcode.POW:
                    if (l == 0 || r == 0)
                        return false;
                    ret = System.Math.Pow(l, r);
                    return true;
                case Instruction.LuaOpcode.CONCAT:
                    break;
                case Instruction.LuaOpcode.EQ:
                    break;
                case Instruction.LuaOpcode.LT:
                    break;
                case Instruction.LuaOpcode.LE:
                    break;
                default:
                    break;
            }
            return false;
        }

        void binOp(string opname, Expression left, Expression right)
        {
            // this and the variable reg2 are saved so that we can use the locations in
            // args B and C to the opcode

            int reg1, reg2;

            double result;
            if (constfolding(left, right, opname, out result) == false)
            {
                if (left is NumberExpr == false)
                {
                    reg1 = block.regnum;
                    DoExpr(left);
                }
                else
                    reg1 = 256 + block.K[number(((NumberExpr)left).Value)];
                if (right is NumberExpr == false)
                {
                    reg2 = block.regnum;
                    DoExpr(right);
                }
                else
                    reg2 = 256 + block.K[number(((NumberExpr)right).Value)];
            }
            else
            {
                DoExpr(new NumberExpr() { Value = result.ToString() });
                return;
            }


            Instruction i = new Instruction(opname);
            // We no longer need reg1 through the top, so we can overwrite them
            block.regnum = reg1;

            i.A = block.getreg();

            // >256 is const

            i.B = reg1;
            i.C = reg2;
            emit(i);
        }

        void unOp(string opname, Expression expr)
        {
            // this and the variable reg2 are saved so that we can use the locations in
            // args B and C to the opcode
            int reg1 = block.regnum;
            DoExpr(expr);
            Instruction i = new Instruction(opname);

            // We no longer need reg1 through the top, so we can overwrite them
            block.regnum = reg1;

            i.A = block.getreg();

            // >256 is const

            i.B = reg1;
            emit(i);
        }
    }
}
