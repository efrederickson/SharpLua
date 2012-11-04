using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.LASM;
using System.Diagnostics;

namespace SharpLua.LASMTests
{
    class Program
    {
        static void Main(string[] args)
        {
            LasmParser p = new LasmParser();
            LuaFile file = p.Parse(@"
        .const ""print""
        .const ""Hello""
        getglobal 0 0
        loadk 1 1
        call 0 2 1
        return 0 1
        ");
            file.StripDebugInfo();
            string code = file.Compile();
            try
            {
            	LuaFile f = Disassembler.Disassemble(code);
                System.IO.FileStream fs = new System.IO.FileStream("lasm.luac", System.IO.FileMode.Create);
                //foreach (char c in code)
                foreach (char c in f.Compile())
                    fs.WriteByte((byte)c);
                fs.Close();

                // Test chunk compiling/loading
                string s = f.Main.Compile(f);
                Chunk chnk = Disassembler.DisassembleChunk(s);

                // Test execution of code
                string s2 = f.Compile();
                LuaRuntime.Run(s2);
                LuaRuntime.Run(code);
                Console.WriteLine("The above line should say 'Hello'. If it doesn't there is an error.");
                Console.WriteLine(LASMDecompiler.Decompile(file));

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Test(s) done");
            Console.Read();
        }
    }
}
