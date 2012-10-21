using System;

namespace SharpLua.LASM
{
    public class Disassembler
    {
        static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        static int index = 0;
        //static bool big = false;
        static LuaFile file = new LuaFile();
        static Func<string, double> loadNumber = null;
        static string chunk = "";

        static string Read(int len)
        {
            string c = chunk.Substring(index, len);
            index += len;
            if (file.BigEndian)
            {
                c = Reverse(c);
            }
            return c;
        }

        static int ReadInt8()
        {
            return (int)Read(1)[0];
        }

        static double ReadNumber()
        {
            return loadNumber(Read(file.NumberSize));
        }

        static string GetString(int len)
        {
            return Read(len);
        }

        static int ReadInt32()
        {
            if (file.IntegerSize > file.SizeT)
                throw new Exception("IntegerSize cannot be greater than SizeT");

            string x = Read(file.SizeT);
            if (x == null || x.Length == 0)
                throw new Exception("Could not load integer");
            else
            {
                long sum = 0;
                for (int i = file.IntegerSize - 1; i >= 0; i--)
                {
                    sum = (sum * 256) + (int)x[i];
                }

                // check for negative number
                if (x[file.IntegerSize - 1] > 127)
                    sum = sum - (long)Bit.ldexp(1, 8 * file.IntegerSize);
                return (int)sum;
            }
        }

        static string ReadString()
        {
            int len = (int)ReadInt32();
            string s = GetString(len);
            // Strip last '\0':
            return s.Length > 0
                ? s.Substring(0, s.Length - 1)
                : s;
        }

        static Chunk ReadFunction()
        {
            Chunk c = new Chunk();
            c.Name = ReadString();
            c.FirstLine = (uint)ReadInt32();
            c.LastLine = (ulong)ReadInt32();
            c.UpvalueCount = ReadInt8(); // Upvalues
            c.ArgumentCount = ReadInt8();
            c.Vararg = ReadInt8();
            c.MaxStackSize = (uint)ReadInt8();

            // Instructions
            long count = ReadInt32();
            for (int i = 0; i < count; i++)
            {
                uint op = (uint)ReadInt32();
                int opcode = (int)Lua.GET_OPCODE(op);
                //(int)Bit.Get(op, 1, 6);
                Instruction instr = Instruction.From(op);
                instr.Number = i;
                c.Instructions.Add(instr);
            }

            // Constants

            count = ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Constant cnst = new Constant(0, null);
                int t = ReadInt8();

                cnst.Number = i;

                if (t == 0)
                {
                    cnst.Type = ConstantType.Nil;
                    cnst.Value = null;
                }
                else if (t == 1)
                {
                    cnst.Type = ConstantType.Bool;
                    cnst.Value = ReadInt8() != 0;
                }
                else if (t == 3)
                {
                    cnst.Type = ConstantType.Number;
                    cnst.Value = ReadNumber();
                }
                else if (t == 4)
                {
                    cnst.Type = ConstantType.String;
                    cnst.Value = ReadString();
                }
                c.Constants.Add(cnst);
            }

            // Protos

            count = ReadInt32();
            for (int i = 0; i < count; i++)
                c.Protos.Add(ReadFunction());

            // Line numbers
            count = ReadInt32();
            for (int i = 0; i < count; i++)
                c.Instructions[i].LineNumber = ReadInt32();

            // Locals
            count = ReadInt32();
            for (int i = 0; i < count; i++)
                c.Locals.Add(new Local(ReadString(), ReadInt32(), ReadInt32()));

            // Upvalues
            count = ReadInt32();
            for (int i = 0; i < count; i++)
                c.Upvalues.Add(new Upvalue(ReadString()));

            return c;
        }

        /// <summary>
        /// Returns a disassembled LuaFile
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static LuaFile Disassemble(string c)
        {
            chunk = c;
            index = 0;
            file = new LuaFile();
            Disassembler.loadNumber = null;
            if (chunk == null || string.IsNullOrWhiteSpace(chunk))
                throw new Exception("chunk is empty");

            file.Identifier = GetString(4); // \027Lua
            if (file.Identifier != (char)27 + "Lua")
                throw new Exception("Not a valid Lua bytecode chunk");

            file.Version = ReadInt8(); // 0x51
            if (file.Version != 0x51)
                throw new Exception(string.Format("Invalid bytecode version, 0x51 expected, got 0x{0:X}", file.Version));
            int fmt = ReadInt8();
            file.Format = fmt == 0 ? Format.Official : Format.Unofficial;
            file.FormatNumber = fmt;
            if (file.Format == Format.Unofficial)
                throw new Exception("Unknown binary chunk format");

            file.BigEndian = ReadInt8() == 0;
            file.IntegerSize = ReadInt8();
            file.SizeT = ReadInt8();
            file.InstructionSize = ReadInt8();
            file.NumberSize = ReadInt8();
            file.IsFloatingPointNumbers = ReadInt8() == 0;
            loadNumber = PlatformConfig.GetNumberTypeConvertFrom(file);
            if (file.InstructionSize != 4)
                throw new Exception("Unsupported instruction size '" + file.InstructionSize + "', expected '4'");
            file.Main = ReadFunction();
            return file;
        }
        
        /// <summary>
        /// Returns a decompiled chunk, with info based upon a default LuaFile
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static Chunk DisassembleChunk(string c)
        {
            chunk = c;
            index = 0;
            file = new LuaFile();
            loadNumber = PlatformConfig.GetNumberTypeConvertFrom(file);
            if (chunk == null || string.IsNullOrWhiteSpace(chunk))
                throw new Exception("chunk is empty");
            return ReadFunction();
        }
    }
}