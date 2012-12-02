using System;
using SharpLua;
namespace SharpLua.LASM
{
    public class LASMDecompiler
    {
        static void decompile(Chunk chunk)
        {
            if (chunk != file.Main)
            {
                write("; Function " + chunk.Name);
                write(".func");
                indent = indent + 1;
            }
            else
                write("; Main code");

            write(".name \"" + chunk.Name + "\"");
            write(".options " + chunk.UpvalueCount + " " + chunk.ArgumentCount + " " + chunk.Vararg + " " + chunk.MaxStackSize);
            write("; Above contains: Upvalue count, Argument count, Vararg flag, Max Stack Size");
            write("");
            if (chunk.Constants.Count > 0)
            {
                write("; Constants");
                foreach (Constant c in chunk.Constants)
                {
                    if (c.Type == ConstantType.Nil)
                        write(".const nil");
                    else if (c.Type == ConstantType.Bool)
                        write(".const " + ((bool)c.Value ? "true" : "false"));
                    else if (c.Type == ConstantType.Number)
                        write(".const " + c.Value);
                    else if (c.Type == ConstantType.String)
                    {
                        // escape string
                        string v = "";
                        foreach (char c2 in (string)c.Value)
                        {
                            int ch = (int)c2;
                            char nC = '\0';
                            // other chars with values > 31 are '"' (34), '\' (92) and > 126
                            if (ch < 32 || ch == 34 || ch == 92 || ch > 126)
                            {
                                if (ch >= 7 && ch <= 13)
                                    nC = "abtnvfr".Substring(ch - 6, 1)[0];
                                else if (ch == 34 || ch == 92)
                                    nC = c2;
                                v = v + "\\" + nC;
                            }
                            else// 32 <= v <= 126 (NOT 255)
                                v = v + c2;
                        }
                        write(".const \"" + v + "\"");
                    }
                }
            }

            if (chunk.Locals.Count > 0)
            {
                write("; Locals");
                foreach (Local l in chunk.Locals)
                    write(".local " + l.Name);
            }

            if (chunk.Upvalues.Count > 0)
            {
                write("; Upvalues");
                foreach (Upvalue u in chunk.Upvalues)
                    write(".upval " + u.Name);
            }

            write("; Instructions");
            foreach (Instruction instr in chunk.Instructions)
            {

                if (instr.OpcodeType == OpcodeType.ABC)
                    write(instr.OpcodeName.ToLower() + " " + instr.A + " " + instr.B + " " + instr.C);
                else if (instr.OpcodeType == OpcodeType.ABx)
                    write(instr.OpcodeName.ToLower() + " " + instr.A + " " + instr.Bx);
                else if (instr.OpcodeType == OpcodeType.AsBx)
                    write(instr.OpcodeName.ToLower() + " " + instr.A + " " + instr.sBx);
            }

            if (chunk.Protos.Count > 0)
            {
                write("; Protos");
                write("");
                foreach (Chunk chunk2 in chunk.Protos)
                    decompile(chunk2);

            }

            if (chunk != file.Main)
            {
                indent--;
                write(".end");
            }
        }

        static int indent = 0;
        static string s = "";
        static LuaFile file;

        static void write(string t)
        {
            s = s + "    ".Repeat(indent) + t + "\r\n";
        }

        public static string Decompile(LuaFile file)
        {
            LASMDecompiler.file = file;
            decompile(file.Main);
            return s;
        }
    }
}