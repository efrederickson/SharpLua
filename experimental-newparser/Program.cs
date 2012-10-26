using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Statement;
using SharpLua.XmlDocumentation;

namespace SharpLua
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRename();
            TestInline();
            TestFindImpl();
            TestFindRef();
            TestExtractDocComments();

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

        private static void TestExtractDocComments()
        {
            string str = @"
---<summary>
--- somefin
---</summary>
---<returns>nothing</returns>
function a()

end

---<summary>
--- ugh.
---</summary>
function tbl.dosomething()

end

---<summary>
--- a var
---</summary>
---<returns>wut?</returns>
x = 1
";

            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex(str));
            Chunk c = p.Parse();
            List<DocumentationComment> docs = ExtractDocumentationComments.Extract(c);
            foreach (DocumentationComment d in docs)
            {
                Console.WriteLine("Documentation comment: " + d.Text + "Var: " + (d.Ident == null ? "<none>" : d.Ident));
            }
            string s = Documentation.Write(docs);
            Console.WriteLine(s);
            Console.WriteLine(Documentation.Read(s).Count);
            if (docs.Count == 0)
                Console.WriteLine("No doc comments. Wut?");
        }

        private static void TestFindRef()
        {
            string s = @"local a = 1
local b = 2
local x, y, z = 1, 2, 3
print(a, b)
a += 3
b += a
print(b, a)
";
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex(s));
            Chunk c = p.Parse();
            Console.WriteLine(s);
            try
            {
                List<Location> l1 = Refactoring.FindReferences(c, c.Scope.GetVariable("a"));
                foreach (Location l2 in l1)
                    Console.WriteLine("Reference of 'a': " + l2.Line + ":" + l2.Column);
                l1 = Refactoring.FindReferences(c, c.Scope.GetVariable("b"));
                foreach (Location l2 in l1)
                    Console.WriteLine("Reference of 'b': " + l2.Line + ":" + l2.Column);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void TestRename()
        {
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex("local a = 5; function c() print(a) end c()"));
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

        static void TestFindImpl()
        {
            string s = @"local a = 1
local b = 2
local x, y, z = 1, 2, 3
print(a, b)
x += 1
print(z)
function print2(...)
    _G.print(...)
end
";
            Lexer l = new Lexer();
            Parser p = new Parser(l.Lex(s));
            Chunk c = p.Parse();
            Console.WriteLine(s);
            Location l1 = Refactoring.FindImplementation(c, c.Scope.GetVariable("a"));
            Console.WriteLine("impl of 'a': " + l1.Line + ":" + l1.Column);
            l1 = Refactoring.FindImplementation(c, c.Scope.GetVariable("b"));
            Console.WriteLine("impl of 'b': " + l1.Line + ":" + l1.Column);
            l1 = Refactoring.FindImplementation(c, c.Scope.GetVariable("x"));
            Console.WriteLine("impl of 'x': " + l1.Line + ":" + l1.Column);
            l1 = Refactoring.FindImplementation(c, c.Scope.GetVariable("z"));
            Console.WriteLine("impl of 'z': " + l1.Line + ":" + l1.Column);
            l1 = Refactoring.FindImplementation(c, c.Scope.GetVariable("print2"));
            Console.WriteLine("impl of 'print2': " + l1.Line + ":" + l1.Column);
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
