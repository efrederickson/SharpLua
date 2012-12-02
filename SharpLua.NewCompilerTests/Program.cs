/*
 * Created by SharpDevelop.
 * User: enoch
 * Date: 11/3/2012
 * Time: 5:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using SharpLua.Ast;
using SharpLua.LASM;

namespace SharpLua.NewCompilerTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    string s = Console.ReadLine();
                    Lexer l = new Lexer();
                    Parser p = new Parser(l.Lex(s));
                    Ast.Chunk c = p.Parse();
                    Compiler.Compiler cplr = new SharpLua.Compiler.Compiler();
                    LuaFile proto = cplr.Compile(c, "<stdin>");
                    Console.WriteLine("compiled!");
                    FileStream fs = File.Open("out.sluac", FileMode.Create);
                    foreach (char ch in proto.Compile())
                        fs.WriteByte((byte)ch);
                    fs.Close();
                    Console.WriteLine("written to out.sluac!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
}