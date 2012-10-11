using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimental_newparser
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {

                Lexer l = new Lexer();
                TokenReader r = l.Lex(Console.ReadLine());
                //Console.WriteLine("---------------------------------");
                foreach (Token t in r.tokens)
                    Console.WriteLine(t.Print());
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
}
