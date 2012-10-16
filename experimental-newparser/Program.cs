using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Statement;

namespace SharpLua
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRename();

            while (true)
            {
                string line = Console.ReadLine();
                try
                {
                    Lexer l = new Lexer();
                    TokenReader r = l.Lex(line);
                    //Console.WriteLine("---------------------------------");
                    foreach (Token t in r.tokens)
                    {
                        Console.WriteLine(t.Print());
                        foreach (Token t2 in t.Leading)
                            Console.WriteLine("    " + t2.Print());
                    }
                    //Console.WriteLine("- PARSER OUTPUT -");
                    Parser p = new Parser(r);
                    Chunk c = p.Parse();
                    //dump(c.Body);
                    Console.WriteLine("- Beautified -");
                    Visitors.Beautifier b = new Visitors.Beautifier();
                    Console.WriteLine(b.Beautify(c));
                    Console.WriteLine("- Lua Compatible -");
                    Visitors.LuaCompatibleOutput lco = new Visitors.LuaCompatibleOutput();
                    Console.WriteLine(lco.Format(c));
                    Console.WriteLine("- Exact reconstruction -");
                    Console.WriteLine(new Visitors.ExactReconstruction().Reconstruct(c));
                    Console.WriteLine("- Minified -");
                    Console.WriteLine(new Visitors.Minifier().Minify(c));
                }
                catch (LuaSourceException ex)
                {
                    Console.WriteLine(line);
                    Console.WriteLine(" ".Repeat(ex.Column - 1) + "^");
                    Console.WriteLine("<stdin>:" + ex.Line + ":" + ex.Column + ":" + ex.Message);
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }

        private static void TestRename()
        {
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex("local a = 5; local function c() print(a) end c()"));
            Chunk c = p.Parse();
            c.Scope.RenameVariable("a", "b");
            c.Scope.RenameVariable("c", "testfunc");
            Visitors.Beautifier e = new Visitors.Beautifier();
            Console.WriteLine(e.Beautify(c));
            Visitors.ExactReconstruction e2 = new Visitors.ExactReconstruction();
            Console.WriteLine(e2.Reconstruct(c));
        }

        static void dump(List<Statement> s)
        {
            foreach (Statement s2 in s)
            {
                dump2(s2);
                Chunk c = s2 as Chunk;
                if (c != null)
                    dump(c.Body);
            }
        }

        static void dump2(Statement s)
        {
            Console.WriteLine(s.GetType().Name);// + " " + s.HasSemicolon);
            foreach (Token x in s.ScannedTokens)
                Console.WriteLine("    " + x.Print());
        }
    }
}
