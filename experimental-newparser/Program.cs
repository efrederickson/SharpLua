using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using experimental_newparser.Ast;
using experimental_newparser.Ast.Statement;

namespace experimental_newparser
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string line = Console.ReadLine();
                try
                {
                    Lexer l = new Lexer();
                    TokenReader r = l.Lex(line);
                    //Console.WriteLine("---------------------------------");
                    foreach (Token t in r.tokens)
                        Console.WriteLine(t.Print());
                    Console.WriteLine("- PARSER OUTPUT -");
                    Parser p = new Parser(r);
                    Chunk c = p.Parse();
                    dump(c.Body);
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
