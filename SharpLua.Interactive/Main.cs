using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SharpLua.Interactive
{
    public class Program
    {
        /// <summary>
        /// The Prompt used in Interactive Mode
        /// </summary>
        public static string Prompt
        { get; set; }

        /// <summary>
        /// A REPL (Read, Eval, Print, Loop function) for #Lua
        /// </summary>
        /// <param name="args">Application startup args</param>
        public static void Main(string[] args)
        {
            // TODO: Better arg parsing/checking, make it more like the C lua

            bool GoInteractive = true;

            // Create global variables
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

#if DEBUG
#if true
            LuaRuntime.RegisterModule(typeof(TestModule));
            LuaRuntime.RegisterModule(typeof(TestModule2));
#endif

            // how to handle errors
            //LuaRuntime.SetVariable("DEBUG", true);
            LuaRuntime.SetVariable("DEBUG", false); 
            // We don't need the C# traceback.
            // All it is is [error]
            //     at LuaInterface.ThrowErrorFromException
            //     [...]
            // Which we don't need
            
#else
            LuaRuntime.SetVariable("DEBUG", false);
#endif

            Prompt = "> ";
            
            bool wasSetInteract = false;
            bool wasFileRun = false;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.ToUpper() == "-I")
                {
                    GoInteractive = true;
                    wasSetInteract = true;
                }
                else if (arg.ToUpper() == "-NOI")
                    GoInteractive = false;
                else if (arg.Substring(0, 2).ToLower() == "-l")
                {
                    if (arg.Length == 2)
                    {
                        string lib = args[++i];
                        LuaRuntime.Require(lib);
                    }
                    else
                    {
                        string lib = arg.Substring(2);
                        LuaRuntime.Require(lib);
                    }
                }
                else if (arg.ToLower() == "-e")
                {
                    if (wasSetInteract == false)
                        GoInteractive = true;
                    
                    LuaRuntime.Run(args[++i]);
                    
                    if (wasSetInteract == false)
                        GoInteractive = false;
                    wasFileRun = true;
                }
                else if (arg == "--")
                    break;
                else
                {
                    LuaTable t = LuaRuntime.GetLua().NewTable("arg");
                    int i3 = 1;
                    if (args.Length > i + 1)
                        for (int i2 = i + 1; i2 < args.Length; i2++)
                            t[i3++] = args[i2];
                    
                    t[-1] = System.Windows.Forms.Application.ExecutablePath;
                    t["n"] = t.Keys.Count;
                    
                    if (File.Exists(args[i]))
                        LuaRuntime.SetVariable("_WORKDIR", Path.GetDirectoryName(args[i]));
                    else
                        LuaRuntime.SetVariable("_WORKDIR", Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
                    LuaRuntime.RunFile(args[i]);
                    
                    if (wasSetInteract == false)
                        GoInteractive = false;
                    
                    wasFileRun = true;
                    break;
                }
            }

            
            /*
            // check command line args
            if (args.Length > 0)
            {
                string file = args[0];
                if (File.Exists(file))
                {
                    try
                    {
                        LuaRuntime.SetVariable("_WORKDIR", Path.GetDirectoryName(file));
                        LuaRuntime.RunFile(file);
                        // it loaded successfully
                        if (args.Length > 1 && args[1].ToUpper() != "-NOI") // possibly interactive mode after
                            Console.WriteLine("Loaded file '" + Path.GetFileName(file) + "'");
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                    //Console.ReadKey(true);
                    //return;
                    // instead, go to interactive
                }
                else
                {
                    Console.WriteLine(file + " not found.");
                }
            }
            */
           
            if (args.Length == 0)
                GoInteractive = true;
            
            if (GoInteractive)
            {
                if (args.Length == 0 || wasFileRun == false)
                    LuaRuntime.PrintBanner();
                LuaRuntime.SetVariable("_WORKDIR", Path.GetDirectoryName(typeof(Program).Assembly.Location));
                while (true)
                {
                    Console.Write(Prompt);
                    string line = Console.ReadLine();

                    if (line == "quit" || line == "exit" || line == "bye")
                    {
                        break;
                    }
                    else
                    {
                        try
                        {
                            object[] v = LuaRuntime.GetLua().DoString(line, "<stdin>");
                            if (v == null || v.Length == 0)
#if DEBUG
                                Console.WriteLine("=> [no returned value]");
#else
                                ;
#endif
                            else
                            {
                                Console.Write("=> ");
                                for (int i = 0; i < v.Length; i++)
                                    if (v[i] == null)
                                        Console.WriteLine("<nil>");
                                    else
                                        Console.Write(v[i].ToString() + (i != v.Length - 1 ? ", " : ""));
                                Console.WriteLine();
                            }
                        }
                        catch (LuaSourceException ex)
                        {
                            for (int i = 1; i < ex.Column; i++)
                                Console.Write(" ");

                            // Offset for prompt
                            for (int i = 0; i < Prompt.Length; i++)
                            {
                                //Console.WriteLine(i);
                                Console.Write(" ");
                            }

                            Console.WriteLine("^");
                            Console.WriteLine(ex.GenerateMessage("<stdin>"));
                        }
                        catch (Exception error)
                        {
                            object dbg = LuaRuntime.GetVariable("DEBUG");

                            if (dbg != null && (dbg is bool && (bool)dbg == true))
                                Console.WriteLine(error.ToString());
                            else
                                Console.WriteLine(error.Message);
                            LuaRuntime.SetVariable("LASTERROR", error);
                        }
                    }
                }
            }
        }
    }

#if true
    // Lua Test Modules
    // Used primarily for testing the LuaModule and LuaFunction attributes.
    // Feel free to expand or delete them.

    [LuaModule("TestModule")]
    class TestModule
    {
        [LuaFunction("test")]
        public static void PrintHi()
        {
            Console.WriteLine("hi");
        }
        
        public object b(object self)
        {
            return self;
        }
        
        public void a(int t, string s, double x, LuaTable tbl, object o, bool b)
        {
            //Console.WriteLine(self);
            Console.WriteLine(t==null);
            Console.WriteLine(s==null);
            Console.WriteLine(x==null);
            Console.WriteLine(tbl==null);
            Console.WriteLine(o==null);
            Console.WriteLine(b==null);
        }

    }

    [LuaModule]
    class TestModule2
    {
        [LuaFunction]
        public static void PrintHi()
        {
            Console.WriteLine("hi");
        }
    }
#endif
}
