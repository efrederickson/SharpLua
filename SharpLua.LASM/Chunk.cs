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
                    for (int i = 0; i < file.IntegerSize; i++)
                    {
                        v += (char)(num % 256);
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
                    {
                        string l = DumpInt(s.Length + 1);
                        return l + s + "\0";
                    }
                });

            string c = "";
            c += DumpString(Name);
            c += DumpInt((int)FirstLine);
            c += DumpInt((int)LastLine);
            c += (char)UpvalueCount;
            c += (char)ArgumentCount;
            c += (char)Vararg;
            c += (char)MaxStackSize;

            // Instructions
            c += DumpInt(Instructions.Count);
            foreach (Instruction i in Instructions)
                c += DumpBinary.Opcode(i);


            // Constants
            c += DumpInt(Constants.Count);
            foreach (Constant cnst in Constants)
            {
                if (cnst.Type == ConstantType.Nil)
                    c += (char)0;
                else if (cnst.Type == ConstantType.Bool)
                {
                    c += (char)1;
                    c += (char)((bool)cnst.Value ? 1 : 0);
                }
                else if (cnst.Type == ConstantType.Number)
                {
                    c += (char)3;
                    c += DumpNumber((long)cnst.Value);
                }
                else if (cnst.Type == ConstantType.String)
                {
                    c += (char)4;
                    c += DumpString((string)cnst.Value);
                }
                else
                    throw new Exception("Invalid constant type: " + cnst.Type.ToString());
            }

            // Protos
            c += DumpInt(Protos.Count);
            foreach (Chunk ch in Protos)
                c += ch.Compile(file);


            // Line Numbers
            c += DumpInt(Instructions.Count);
            foreach (Instruction i in Instructions)
                c += DumpInt(i.LineNumber);


            // Locals 
            c += DumpInt(Locals.Count);
            foreach (Local l in Locals)
            {
                c += DumpString(l.Name);
                c += DumpInt(l.StartPC);
                c += DumpInt(l.EndPC);
            }

            // Upvalues
            c += DumpInt(Upvalues.Count);
            foreach (Upvalue v in Upvalues)
                c += DumpString(v.Name);
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