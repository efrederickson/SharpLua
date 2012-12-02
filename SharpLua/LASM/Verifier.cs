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
                    case Instruction.LuaOpcode.MOVE:
                        assert(i.C == 0, "MOVE.C must equal 0");
                        assert(i.A < c.MaxStackSize, "MOVE.A out of bounds");
                        assert(i.B < c.MaxStackSize, "MOVE.B out of bounds");
                        break;

                    case Instruction.LuaOpcode.LOADK:
                        assert(i.A < c.MaxStackSize, "LOADK.A out of bounds");
                        assert(i.Bx < c.Constants.Count, "LOADK.Bx out of bounds");
                        break;

                    case Instruction.LuaOpcode.LOADBOOL:
                        assert(i.A < c.MaxStackSize, "LOADBOOL.A out of bounds");
                        assert(i.B < 2, "LOADBOOL.B invalid value");
                        assert(i.C < 2, "LOADBOOL.C invalid value");
                        break;

                    case Instruction.LuaOpcode.LOADNIL:
                        assert(i.A < c.MaxStackSize, "LOADNIL.A out of bounds");
                        assert(i.B < c.MaxStackSize, "LOADNIL.B out of bounds");
                        break;

                    case Instruction.LuaOpcode.GETUPVAL:
                        assert(i.A < c.MaxStackSize, "GETUPVAL.A out of bounds");
                        assert(i.B < c.Upvalues.Count, "GETUPVAL.B out of bounds");
                        break;

                    case Instruction.LuaOpcode.GETGLOBAL:
                        break;

                    case Instruction.LuaOpcode.GETTABLE:
                        break;

                    case Instruction.LuaOpcode.SETGLOBAL:
                        break;

                    case Instruction.LuaOpcode.SETUPVAL:
                        break;

                    case Instruction.LuaOpcode.SETTABLE:
                        break;

                    case Instruction.LuaOpcode.NEWTABLE:
                        break;

                    case Instruction.LuaOpcode.SELF:
                        break;

                    case Instruction.LuaOpcode.ADD:
                        break;

                    case Instruction.LuaOpcode.SUB:
                        break;

                    case Instruction.LuaOpcode.MUL:
                        break;

                    case Instruction.LuaOpcode.DIV:
                        break;

                    case Instruction.LuaOpcode.MOD:
                        break;

                    case Instruction.LuaOpcode.POW:
                        break;

                    case Instruction.LuaOpcode.UNM:
                        break;

                    case Instruction.LuaOpcode.NOT:
                        break;

                    case Instruction.LuaOpcode.LEN:
                        break;

                    case Instruction.LuaOpcode.CONCAT:
                        break;

                    case Instruction.LuaOpcode.JMP:
                        break;

                    case Instruction.LuaOpcode.EQ:
                        break;

                    case Instruction.LuaOpcode.LT:
                        break;

                    case Instruction.LuaOpcode.LE:
                        break;

                    case Instruction.LuaOpcode.TEST:
                        break;

                    case Instruction.LuaOpcode.TESTSET:
                        break;

                    case Instruction.LuaOpcode.CALL:
                        break;

                    case Instruction.LuaOpcode.TAILCALL:
                        break;

                    case Instruction.LuaOpcode.RETURN:
                        break;

                    case Instruction.LuaOpcode.FORLOOP:
                        break;

                    case Instruction.LuaOpcode.FORPREP:
                        break;

                    case Instruction.LuaOpcode.TFORLOOP:
                        break;

                    case Instruction.LuaOpcode.SETLIST:
                        break;

                    case Instruction.LuaOpcode.CLOSE:
                        break;

                    case Instruction.LuaOpcode.CLOSURE:
                        break;

                    case Instruction.LuaOpcode.VARARG:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}