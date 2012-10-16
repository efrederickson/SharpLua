using System;
using System.Collections.Generic;
namespace SharpLua.LASM
{
    public class Chunk
    {
        public string Name = "";
        public uint FirstLine = 1;
        public long LastLine = 1;
        public int UpvalueCount = 0;
        public int ArgumentCount = 0;
        public int Vararg = 0;
        public uint MaxStackSize = 10;
        public List<Instruction> Instructions = new List<Instruction>();
        public List<Constant> Constants = new List<Constant>();
        public List<Chunk> Protos = new List<Chunk>();
        public List<Local> Locals = new List<Local>();
        public List<Upvalue> Upvalues = new List<Upvalue>();

        public string Compile(LuaFile file)
        {
            Func<double, string> DumpNumber = PlatformConfig.GetNumberTypeConvertTo(file);

            Func<int, string> DumpInt = new Func<int, string>(delegate(int num)
                {
                    string v = "";
                    for (int i = 1; i < file.IntegerSize; i++)
                    {
                        v = v + (char)(num % 256);
                        num = (int)Math.Floor((double)num / 256);
                    }
                    return v;
                });

            Func<string, string> DumpString = new Func<string, string>(delegate(string s)
                {
                    int len = file.SizeT;
                    if (s == null || s.Length == 0)
                        return "\0".Repeat(len);
                    else
                        return DumpInt(s.Length + 1) + s + "\0";
                });

            string c = "";
            c = c + DumpString(Name);
            c = c + DumpInt((int)FirstLine);
            c = c + DumpInt((int)LastLine);
            c = c + (char)UpvalueCount;
            c = c + (char)ArgumentCount;
            c = c + (char)Vararg;
            c = c + (char)MaxStackSize;

            // Instructions
            c = c + DumpInt(Instructions.Count);
            foreach (Instruction i in Instructions)
                c = c + DumpBinary.Opcode(i);


            // Constants
            c = c + DumpInt(Constants.Count);
            foreach (Constant cnst in Constants)
            {
                if (cnst.Type == ConstantType.Nil)
                    c = c + (char)0;
                else if (cnst.Type == ConstantType.Bool)
                {
                    c = c + (char)1;
                    c = c + (char)((bool)cnst.Value ? 1 : 0);
                }
                else if (cnst.Type == ConstantType.Number)
                {
                    c = c + (char)3;
                    c = c + DumpNumber((long)cnst.Value);
                }
                else if (cnst.Type == ConstantType.String)
                {
                    c = c + (char)4;
                    c = c + DumpString((string)cnst.Value);
                }
                else
                    throw new Exception("Invalid constant type: " + cnst.Type.ToString());
            }

            // Protos
            c = c + DumpInt(Protos.Count);
            foreach (Chunk ch in Protos)
                c = c + ch.Compile(file);


            // Line Numbers
            c = c + DumpInt(Instructions.Count);
            foreach (Instruction i in Instructions)
                c = c = DumpInt(i.LineNumber);


            // Locals 
            c = c + DumpInt(Locals.Count);
            foreach (Local l in Locals)
            {
                c = c + DumpString(l.Name);
                c = c + DumpInt(l.StartPC);
                c = c + DumpInt(l.EndPC);
            }

            // Upvalues
            c = c + DumpInt(Upvalues.Count);
            foreach (Upvalue v in Upvalues)
                c = c + DumpString(v.Name);
            return c;
        }

        public void StripDebugInfo()
        {
            Name = "";
            FirstLine = 1;
            LastLine = 1;
            foreach (Instruction i in Instructions)
                i.LineNumber = 0;
            Locals.Clear();
            foreach (Chunk p in Protos)
                p.StripDebugInfo();
            Upvalues.Clear();
        }

        public void Verify()
        {
            Verifier.VerifyChunk(this);
            foreach (Chunk c in Protos)
                c.Verify();
        }
    }
}