using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Decompiler
{
    /// <summary>
    /// Incredibly simple Lua decompiler
    /// </summary>
    public class Decompiler
    {
        StringBuilder sb = new StringBuilder();
        Dictionary<int, string> adr2Name = new Dictionary<int, string>();
        Lua.Proto f;

        void emit(string s)
        {
            sb.Append(s);
        }

        int nameIndex = 0;
        string getName(int i)
        {
            if (adr2Name.ContainsKey(i))
                return adr2Name[i];
            else
            {
                adr2Name.Add(i, "var" + nameIndex++);
                return adr2Name[i];
            }
        }

        void setName(int i, string name)
        {
            if (adr2Name.ContainsKey(i))
                adr2Name[i] = name;
            else
                adr2Name.Add(i, name);
        }

        void emitConst(int bx, bool quoteStrings)
        {
            Lua.lua_TValue val = f.k[bx];
            Lua.Value v = val.value;
            if (val.tt == Lua.LUA_TSTRING)
                if (quoteStrings)
                    emit("\"" + v.p.ToString() + "\""); // TODO: escape
                else
                    emit(v.p.ToString());
            else if (val.tt == Lua.LUA_TBOOLEAN)
                emit(v.n == 0 ? "false" : "true");
            else if (val.tt == Lua.LUA_TNUMBER)
                emit(v.p.ToString());
            else
                emit("nil");
        }

        void emitName(int a)
        {
            emit(getName(a));
        }

        // 2 : const
        // 1 : ref
        // 0 : fatal error in C#
        int isRegOrConst(int a)
        {
            if (a > 255)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        void emitRegOrConst(int a)
        {
            if (a > 255)
            {
                emitConst(a - 256, true);
            }
            else
            {
                emitName(a);
            }
        }

        public string Decompile(Lua.Proto f)
        {
            this.f = f;

            foreach (uint cd in f.code)
            {
                Lua.OpCode op = Lua.GET_OPCODE(cd);
                Lua.OpMode mode = Lua.getOpMode(op);

                int a = Lua.GETARG_A(cd);
                int b = Lua.GETARG_B(cd);
                int c = Lua.GETARG_C(cd);
                int bx = Lua.GETARG_Bx(cd);

                switch (op)
                {
                    case Lua.OpCode.OP_MOVE:
                        emitName(a);
                        emit(" = ");
                        emitName(b);
                        break;
                    case Lua.OpCode.OP_LOADK:
                        emitName(a);
                        emit(" = ");
                        emitConst(bx, true);
                        break;
                    case Lua.OpCode.OP_LOADBOOL:
                        emitName(a);
                        emit(" = ");
                        if (b == 0)
                            emit("false");
                        else
                            emit("true");
                        break;
                    case Lua.OpCode.OP_LOADNIL:
                        for (int _ = a; _ < b; _++)
                        {
                            emitName(_);
                            emit(" = nil");
                        }
                        break;
                    case Lua.OpCode.OP_GETUPVAL:
                        emitName(a);
                        emit(" = ");
                        emit(f.upvalues[b].str.ToString());
                        break;
                    case Lua.OpCode.OP_GETGLOBAL:
                        emit(getName(a));
                        emit(" = ");
                        emitConst(bx, false);
                        break;
                    case Lua.OpCode.OP_GETTABLE:
                        emitName(a);
                        emit(" = ");
                        emitName(b);
                        emit("[");
                        emitRegOrConst(c);
                        emit("]");
                        break;
                    case Lua.OpCode.OP_SETGLOBAL:
                        emitConst(bx, false);
                        emit(" = ");
                        emitName(a);
                        break;
                    case Lua.OpCode.OP_SETUPVAL:
                        emit(f.upvalues[b].str.ToString());
                        emit(" = ");
                        emitName(a);
                        break;
                    case Lua.OpCode.OP_SETTABLE:
                        emitName(a);
                        emit("[");
                        emitRegOrConst(b);
                        emit("] = ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_NEWTABLE:
                        emitName(a);
                        emit(" = { }");
                        break;
                    case Lua.OpCode.OP_SELF:
                        // A B C, R(A+1) := R(B); R(A) := R(B)[RK(C)]
                        emitName(a + 1);
                        emit(" = ");
                        emitName(b);

                        emitName(a);
                        emit(" = ");
                        emitName(b);
                        emit("[");
                        emitRegOrConst(c);
                        emit("]");
                        break;
                    case Lua.OpCode.OP_ADD:
                        // A B C, R(A) := RK(B) + RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" + ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_SUB:
                        // A B C, R(A) := RK(B) - RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" - ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_MUL:
                        // A B C, R(A) := RK(B) * RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" * ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_DIV:
                        // A B C, R(A) := RK(B) / RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" / ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_MOD:
                        // A B C, R(A) := RK(B) % RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" % ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_POW:
                        // A B C, R(A) := RK(B) ^ RK(C)
                        emitName(a);
                        emit(" = ");
                        emitRegOrConst(b);
                        emit(" ^ ");
                        emitRegOrConst(c);
                        break;
                    case Lua.OpCode.OP_UNM:
                        //A B, R(A) := -R(B)
                        emitName(a);
                        emit(" = -");
                        emitName(b);
                        break;
                    case Lua.OpCode.OP_NOT:
                        //A B, R(A) := -R(B)
                        emitName(a);
                        emit(" = not ");
                        emitName(b);
                        break;
                    case Lua.OpCode.OP_LEN:
                        //A B, R(A) := #R(B)
                        emitName(a);
                        emit(" = #");
                        emitName(b);
                        break;
                    case Lua.OpCode.OP_CONCAT:
                        // A B C, R(A) := R(B).. ... ..R(C)
                        emitName(a);
                        emit(" = ");
                        for (int _ = b; _ < c; _++)
                        {
                            emitName(_);
                            if (_ != c)
                                emit(" .. ");
                        }
                        emitName(c);
                        break;
                    case Lua.OpCode.OP_JMP:
                        emit("-- JMP Opcode not supported\r\n");
                        int x = Lua.GETARG_sBx(cd);
                        emit("-- JMP " + x);
                        break;
                    case Lua.OpCode.OP_EQ:
                        // A B C, if ((RK(B) == RK(C)) ~= A) then pc++
                        emit("if (");
                        emitRegOrConst(b);
                        emit(" == ");
                        emitRegOrConst(c);
                        emit(") ~= ");
                        emit(a != 0 ? "true" : "false");
                        emit(" then");
                        break;
                    case Lua.OpCode.OP_LT:
                        // A B C, if ((RK(B) < RK(C)) ~= A) then pc++
                        emit("if (");
                        emitRegOrConst(b);
                        emit(" < ");
                        emitRegOrConst(c);
                        emit(") ~= ");
                        emit(a != 0 ? "true" : "false");
                        emit(" then");
                        break;
                    case Lua.OpCode.OP_LE:
                        // A B C, if ((RK(B) == RK(C)) ~= A) then pc++
                        emit("if (");
                        emitRegOrConst(b);
                        emit(" <= ");
                        emitRegOrConst(c);
                        emit(") ~= ");
                        emit(a != 0 ? "true" : "false");
                        emit(" then");
                        break;
                    case Lua.OpCode.OP_TEST:
                        // A C, if not (R(A) <=> C) then pc++
                        emit("if not (");
                        emitName(a);
                        emit(" <= ");
                        emitName(c);
                        emit(" or ");
                        emitName(a);
                        emit(" >= ");
                        emitName(c);
                        emit(") then \r\n");
                        emit("-- Unsupported: pc++");
                        break;
                    case Lua.OpCode.OP_TESTSET:
                        // A B C, if (R(B) <=> C) then R(A) := R(B) else pc++
                        emit("if ");
                        emitName(b);
                        emit(" <= ");
                        emitName(c);
                        emit(" or ");
                        emitName(b);
                        emit(" >= ");
                        emitName(c);
                        emit(" then \r\n");

                        emitName(a);
                        emit(" = ");
                        emitName(b);
                        emit("\r\nend");
                        break;
                    case Lua.OpCode.OP_CALL:
                        // A B C, R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1))
                        bool wroteName = false;
                        for (int _ = a; _ < (a + c) - 2; _++)
                        {
                            wroteName = true;
                            if (_ != (a + c) - 2)
                                emit(", ");
                        }
                        //emitName((a + c) - 2);
                        if (wroteName)
                            emit(" = ");
                        emitName(a);
                        emit("(");
                        for (int _ = a + 1; _ < (a + b) - 1; _++)
                        {
                            emitName(_);
                            if (_ != (a + b) - 1)
                                emit(", ");
                        }
                        emitName(a + b - 1);
                        emit(")");
                        break;
                    case Lua.OpCode.OP_TAILCALL:
                        // A B C, return R(A)(R(A+1), ... ,R(A+B-1))
                        emit("return ");
                        emitName(a);
                        emit("(");
                        for (int _ = a + 1; _ < a + b - 1; _++)
                        {
                            emitName(_);
                            if (_ != a + b - 1)
                                emit(", ");
                        }
                        emit(")");
                        break;
                    case Lua.OpCode.OP_RETURN:
                        // A B, return R(A), ... ,R(A+B-2)
                        emit("return ");
                        for (int _ = a; _ < a + b - 2; _++)
                        {
                            emitName(_);
                            if (_ != a + b - 2)
                                emit(", ");
                        }
                        break;
                    case Lua.OpCode.OP_FORLOOP:
                        emit("-- Unsupported Opcode: FORPREP");
                        break;
                    case Lua.OpCode.OP_FORPREP:
                        emit("-- Unsupported Opcode: FORPREP");
                        break;
                    case Lua.OpCode.OP_TFORLOOP:
                        /*	A C	R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));
								if R(A+3) ~= nil then R(A+2)=R(A+3) else pc++	*/
                        for (int _ = a + 3; _ < a + 2 + c; _++)
                        {
                            emitName(_);
                            if (_ != a + 2 + c)
                                emit(", ");
                        }
                        emit(" = ");
                        emitName(a);
                        emit("(");
                        emitName(a + 1);
                        emit(", ");
                        emitName(a + 2);
                        emit(")\r\nif ");
                        emitName(a + 3);
                        emit(" ~= nil then ");
                        emitName(a + 2);
                        emit(" = ");
                        emitName(a + 3);
                        emit(" end");
                        break;
                    case Lua.OpCode.OP_SETLIST:
                        emit("-- Unsupported Opcode: SETLIST");
                        break;
                    case Lua.OpCode.OP_CLOSE:
                        // A, close all variables in the stack up to (>=) R(A)
                        emit("-- Unsupported Opcode: CLOSE");
                        break;
                    case Lua.OpCode.OP_CLOSURE:
                        // A Bx, R(A) := closure(KPROTO[Bx], R(A), ... ,R(A+n))
                        emit("-- Unsupported Opcode: CLOSURE");
                        break;
                    case Lua.OpCode.OP_VARARG:
                        // A B, R(A), R(A+1), ..., R(A+B-1) = vararg
                        for (int _ = a; _ < a + b - 1; _++)
                        {
                            emitName(_);
                            if (_ != a + b - 1)
                                emit(", ");
                        }
                        emit(" = ...");
                        break;
                    default:
                        break;
                }
                emit("\r\n");
            }
            return sb.ToString();
        }
    }
}
