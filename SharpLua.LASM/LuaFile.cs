using System;
namespace SharpLua.LASM
{
    public class LuaFile
    {
        public string Identifier = (char)27 + "Lua";
        public int Version = 0x51;
        public Format Format = Format.Official;
        public int FormatNumber = 0;
        public bool BigEndian = false;
        public int IntegerSize = 4;
        public int SizeT = 4;
        public int InstructionSize = 4;
        public int NumberSize = 8;
        public bool IsFloatingPointNumbers = true;
        public Chunk Main = new Chunk();

        public string Compile()
        {
            string c = "";
            c += Identifier;
            c += (char)Version; // Should be 0x51
            c += (char)(Format == Format.Official ? 0 : FormatNumber);
            c += (char)(BigEndian ? 0 : 1);
            c += (char)IntegerSize;
            c += (char)SizeT;
            c += (char)InstructionSize;
            c += (char)NumberSize;
            c += (char)(IsFloatingPointNumbers ? 0 : 1);
            // Main function
            c += Main.Compile(this);
            return c;
        }

        public void StripDebugInfo()
        {
            if (Main != null)
                Main.StripDebugInfo();
        }

        public void Verify()
        {
            if (Main != null)
                Main.Verify();
        }
    }
}