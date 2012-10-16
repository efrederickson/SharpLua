using System;

namespace SharpLua.LASM
{
    public class Verifier
    {
        private static void assert(bool x, string msg)
        {
            if (x == false)
                throw new Exception(msg);
        }

        public static void VerifyChunk(Chunk c)
        {
            foreach (Instruction i in c.Instructions)
            {
                switch (i.Opcode)
                {
                    case Instruction.LuaOp.MOVE:
                        assert(i.C == 0, "MOVE.C must equal 0");
                        assert(i.A < c.MaxStackSize, "MOVE.A out of bounds");
                        assert(i.B < c.MaxStackSize, "MOVE.B out of bounds");
                        break;

                    case Instruction.LuaOp.LOADK:
                        assert(i.A < c.MaxStackSize, "LOADK.A out of bounds");
                        assert(i.Bx < c.Constants.Count, "LOADK.Bx out of bounds");
                        break;

                    case Instruction.LuaOp.LOADBOOL:
                        assert(i.A < c.MaxStackSize, "LOADBOOL.A out of bounds");
                        assert(i.B < 2, "LOADBOOL.B invalid value");
                        assert(i.C < 2, "LOADBOOL.C invalid value");
                        break;

                    case Instruction.LuaOp.LOADNIL:
                        assert(i.A < c.MaxStackSize, "LOADNIL.A out of bounds");
                        assert(i.B < c.MaxStackSize, "LOADNIL.B out of bounds");
                        break;

                    case Instruction.LuaOp.GETUPVAL:
                        assert(i.A < c.MaxStackSize, "GETUPVAL.A out of bounds");
                        assert(i.B < c.Upvalues.Count, "GETUPVAL.B out of bounds");
                        break;

                    case Instruction.LuaOp.GETGLOBAL:
                        break;

                    case Instruction.LuaOp.GETTABLE:
                        break;

                    case Instruction.LuaOp.SETGLOBAL:
                        break;

                    case Instruction.LuaOp.SETUPVAL:
                        break;

                    case Instruction.LuaOp.SETTABLE:
                        break;

                    case Instruction.LuaOp.NEWTABLE:
                        break;

                    case Instruction.LuaOp.SELF:
                        break;

                    case Instruction.LuaOp.ADD:
                        break;

                    case Instruction.LuaOp.SUB:
                        break;

                    case Instruction.LuaOp.MUL:
                        break;

                    case Instruction.LuaOp.DIV:
                        break;

                    case Instruction.LuaOp.MOD:
                        break;

                    case Instruction.LuaOp.POW:
                        break;

                    case Instruction.LuaOp.UNM:
                        break;

                    case Instruction.LuaOp.NOT:
                        break;

                    case Instruction.LuaOp.LEN:
                        break;

                    case Instruction.LuaOp.CONCAT:
                        break;

                    case Instruction.LuaOp.JMP:
                        break;

                    case Instruction.LuaOp.EQ:
                        break;

                    case Instruction.LuaOp.LT:
                        break;

                    case Instruction.LuaOp.LE:
                        break;

                    case Instruction.LuaOp.TEST:
                        break;

                    case Instruction.LuaOp.TESTSET:
                        break;

                    case Instruction.LuaOp.CALL:
                        break;

                    case Instruction.LuaOp.TAILCALL:
                        break;

                    case Instruction.LuaOp.RETURN:
                        break;

                    case Instruction.LuaOp.FORLOOP:
                        break;

                    case Instruction.LuaOp.FORPREP:
                        break;

                    case Instruction.LuaOp.TFORLOOP:
                        break;

                    case Instruction.LuaOp.SETLIST:
                        break;

                    case Instruction.LuaOp.CLOSE:
                        break;

                    case Instruction.LuaOp.CLOSURE:
                        break;

                    case Instruction.LuaOp.VARARG:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}