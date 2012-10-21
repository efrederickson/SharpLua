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
            TestInline();

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
                    Console.WriteLine("- Basic Beautifier (No Token Stream) -");
                    Visitors.BasicBeautifier b = new Visitors.BasicBeautifier();
                    Console.WriteLine(b.Beautify(c));
                    Console.WriteLine("- Lua Compatible -");
                    Visitors.LuaCompatibleOutput lco = new Visitors.LuaCompatibleOutput();
                    Console.WriteLine(lco.Format(c));
                    Console.WriteLine("- Exact reconstruction -");
                    Console.WriteLine(new Visitors.ExactReconstruction().Reconstruct(c));
                    Console.WriteLine("- Minified -");
                    Console.WriteLine(new Visitors.Minifier().Minify(c));
                    Console.WriteLine("- Beautifier -");
                    Console.WriteLine(new Visitors.Beautifier().Beautify(c));
                    Console.WriteLine("- Misspelled Variables -");
                    List<Tuple<Variable, Variable>> vars = Refactoring.FindMisspelledVariables(c);
                    //Console.WriteLine(vars.Count);
                    foreach (Tuple<Variable, Variable> v in vars)
                    {
                        Console.WriteLine(v.Item1.Name + " is close to " + v.Item2.Name);
                        Console.Write("\t");
                        if (v.Item1.References > v.Item2.References)
                            Console.WriteLine(v.Item1.Name + " is the best match with " + v.Item1.References + " references");
                        else if (v.Item1.References < v.Item2.References)
                            Console.WriteLine(v.Item2.Name + " is the best match with " + v.Item2.References + " references");
                        else
                            Console.WriteLine("Both have the same amount of references (" + v.Item1.References + ")!");

                    }
                    List<Variable> unused = Refactoring.FindUnusedVariables(c);
                    Console.WriteLine("- Unused Variables -");
                    foreach (Variable v in unused)
                        Console.WriteLine("  " + v.Name);

                    unused = Refactoring.FindUnusedLocalVariables(c);
                    Console.WriteLine("- Unused Local Variables -");
                    foreach (Variable v in unused)
                        Console.WriteLine("  " + v.Name);

                    Refactoring.AddModuleDependency(c, "module1");
                    Refactoring.AddModuleDependency(c, "module2", "local_module2");

                    Refactoring.AddClrDependency(c, "AClrLib", "AClrLib.NamespaceA.AClrType");

                    Console.WriteLine("- With Added Dependencies -");
                    Console.WriteLine(new Visitors.Beautifier().Beautify(c));

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
        }

        private static void TestRename()
        {
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex("local a = 5; local function c() print(a) end c()"));
            Chunk c = p.Parse();
            c.Scope.RenameVariable("a", "b");
            c.Scope.RenameVariable("c", "testfunc");
            Visitors.BasicBeautifier e = new Visitors.BasicBeautifier();
            Console.WriteLine(e.Beautify(c));
            Visitors.ExactReconstruction e2 = new Visitors.ExactReconstruction();
            Console.WriteLine(e2.Reconstruct(c));
        }

        static void TestInline()
        {
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex("local b = function(a, ...) return a + 1, ... end"));
            Chunk c = p.Parse();
            AssignmentStatement a = c.Body[0] as AssignmentStatement;
            a.Rhs[0] = Refactoring.InlineFunction(a.Rhs[0] as SharpLua.Ast.Expression.AnonymousFunctionExpr);
            Visitors.BasicBeautifier e = new Visitors.BasicBeautifier();
            a.ScannedTokens = null;
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
