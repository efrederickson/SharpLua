using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.LASM;

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
            string code = file.Compile();
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream("lasm.luac", System.IO.FileMode.Create);
                foreach (char c in code)
                    fs.WriteByte((byte)c);
                fs.Close();

                LuaFile f = Disassembler.Disassemble(code);
                LuaRuntime.Run(code);
                Console.WriteLine("The above line should say 'Hello'. If it doesn't there is an error.");
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
