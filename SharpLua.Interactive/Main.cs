using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;
using System.Windows.Forms;
using System.IO;

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
            bool GoInteractive = true;

            // Create global variables
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LuaRuntime.PrintBanner();
            LuaRuntime.RegisterModule(typeof(TestModule));
            LuaRuntime.RegisterModule(typeof(TestModule2));

            // how to handle errors
#if DEBUG
            LuaRuntime.SetVariable("DEBUG", true);
#else
            LuaRuntime.SetVariable("DEBUG", false);
#endif

            Prompt = "> ";

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
                        if (args.Length > 1) // possibly interactive mode after
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

            if (args.Length == 0)
                GoInteractive = true;

            if (GoInteractive)
            {
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
                                Console.WriteLine("=> [no returned value]");
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
                        catch (Exception error)
                        {
                            // TODO: show lua script traceback
                            // Possible fix... this isn't safe though.

                            Console.WriteLine(error.Message);
                            LuaRuntime.Run("pcall(function() print(debug.traceback()) end)");
                            Console.WriteLine();

                            object dbg = LuaRuntime.GetVariable("DEBUG");

                            if ((dbg is bool && (bool)dbg == true) || dbg is int == false)
                                ; //Console.WriteLine(error.ToString());
                            else
                                ; // Console.WriteLine("Error: " + error.Message);
                            LuaRuntime.SetVariable("LASTERROR", error);
                        }
                    }
                }
            }
        }
    }

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
}
