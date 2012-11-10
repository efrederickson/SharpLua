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

            LuaRuntime.PrintBanner();

            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

#if DEBUG
#if false
            LuaRuntime.RegisterModule(typeof(TestModule));
            LuaRuntime.RegisterModule(typeof(TestModule2));
#endif

            // how to handle errors
            LuaRuntime.SetVariable("DEBUG", true);
#else
            LuaRuntime.SetVariable("DEBUG", false);
#endif

            Prompt = "> ";

            LuaTable t = LuaRuntime.GetLua().NewTable("arg");
            for (int i = 0; i < args.Length; i++)
                t[i] = args[i];
            t[-1] = System.Windows.Forms.Application.ExecutablePath;
            t["n"] = args.Length - 1;

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

            // check for interactive mode
            foreach (string arg in args)
                if (arg.ToUpper() == "-I")
                    GoInteractive = true;
                else if (arg.ToUpper() == "-NOI")
                    GoInteractive = false;

            if (args.Length == 0)
                GoInteractive = true;

            if (GoInteractive)
            {
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
                            object[] v = LuaRuntime.Run(line);
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
                            // TODO: show lua script traceback
                            // Possible fix... this isn't safe though.

                            //Console.WriteLine(error.Message);
                            // doesnt work :(
                            //LuaRuntime.Run("pcall(function() print(debug.traceback()) end)");
                            //Console.WriteLine();

                            //Lua.LuaState l = LuaRuntime.GetLua().LuaState;
                            //LuaRuntime.GetLua().traceback(l);
                            //Lua.lua_getglobal(l, "print");
                            //Lua.lua_call(l, 1, 0);

                            object dbg = LuaRuntime.GetVariable("DEBUG");

                            if ((dbg is bool && (bool)dbg == true) || dbg is int == false)
                                Console.WriteLine(error.ToString());
                            else
                                Console.WriteLine("Error: " + error.Message);
                            LuaRuntime.SetVariable("LASTERROR", error);
                        }
                    }
                }
            }
        }
    }

#if false
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
