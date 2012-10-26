using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpLua;
using SharpLua.Ast;
using SharpLua.XmlDocumentation;

namespace GenerateDocumentation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Lexer l = new Lexer();
                Parser p = new Parser(l.Lex(File.ReadAllText(args[0])));
                Chunk c = p.Parse();
                var doc = ExtractDocumentationComments.Extract(c);
                File.WriteAllText(Path.ChangeExtension(args[0], ".xml"),
                    Documentation.Write(doc));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
