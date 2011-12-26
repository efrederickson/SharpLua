using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SharpLua.Library;
using SharpLua.LuaTypes;

namespace SharpLua
{
    public class LuaRuntime
    {
        public static string Prompt
        {get; set; }
        public static LuaTable GlobalEnvironment;
        private static bool GoInteractive = false;
        
        public static LuaValue RunFile(string luaFile)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile));
        }

        public static LuaValue RunFile(string luaFile, LuaTable enviroment)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile), enviroment);
        }

        public static LuaValue Run(string luaCode)
        {
            return Run(luaCode, CreateGlobalEnviroment());
        }

        public static LuaValue Run(string luaCode, LuaTable enviroment)
        {
            Chunk chunk = Parse(luaCode);
            chunk.Enviroment = enviroment;
            return chunk.Execute();
        }

        static Parser.Parser parser = new Parser.Parser();

        public static Chunk Parse(string luaCode)
        {
            // remove previous errors
            parser.Errors.Clear();
            
            bool success;
            Chunk chunk = parser.ParseChunk(new Parser.TextInput(luaCode), out success);
            if (success)
            {
                return chunk;
            }
            else
            {
                throw new ArgumentException("Code has syntax errors:\r\n" + parser.GetErrorMessages());
            }
        }

        public static LuaTable CreateGlobalEnviroment()
        {
            LuaTable global = new LuaTable();
            
            // Register Lua Module
            BaseLib.RegisterFunctions(global);
            StringLib.RegisterModule(global);
            TableLib.RegisterModule(global);
            IOLib.RegisterModule(global);
            FileLib.RegisterModule(global);
            MathLib.RegisterModule(global);
            OSLib.RegisterModule(global);
            ScriptLib.RegisterModule(global);
            WinFormLib.RegisterModule(global);
            ConsoleLib.RegisterModule(global);
            GarbageCollectorLib.RegisterModule(global);
            CoroutineLib.RegisterModule(global);
            PackageLib.RegisterModule(global);
            ClassLib.RegisterModule(global);
            
            global.SetNameValue("_WORKDIR", new LuaString(Application.StartupPath + "\\"));
            global.SetNameValue("_VERSION", new LuaString("Sharp Lua 1.0"));
            global.SetNameValue("_G", global);
            // set package.preload table
            LuaTable preload = (LuaTable) (global.GetValue("package") as LuaTable).GetValue("preload");
            preload.SetNameValue("string", (LuaTable) global.GetValue("string"));
            preload.SetNameValue("table", (LuaTable) global.GetValue("table"));
            preload.SetNameValue("io", (LuaTable) global.GetValue("io"));
            preload.SetNameValue("file", (LuaTable) global.GetValue("file"));
            preload.SetNameValue("math", (LuaTable) global.GetValue("math"));
            preload.SetNameValue("os", (LuaTable) global.GetValue("os"));
            preload.SetNameValue("script", (LuaTable) global.GetValue("script"));
            preload.SetNameValue("WinForms", (LuaTable) global.GetValue("WinForms"));
            preload.SetNameValue("console", (LuaTable) global.GetValue("console"));
            preload.SetNameValue("coroutine", (LuaTable) global.GetValue("coroutine"));
            preload.SetNameValue("package", (LuaTable) global.GetValue("package"));
            preload.SetNameValue("class", (LuaTable) global.GetValue("class"));
            
            GlobalEnvironment = global;
            
            return global;
        }
        
        public static string FindFullPath(string spath)
        {
            if (File.Exists(spath))
                return spath;
            if (File.Exists(spath + ".lua")) // lua
                return spath + ".lua";
            if (File.Exists(spath + ".wlua"))
                return spath + ".wlua"; // wLua
            if (File.Exists(spath + ".slua")) // sLua (SharpLua)
                return spath + ".slua";
            if (File.Exists(spath + ".dll"))
                return spath + ".dll";
            if (File.Exists(spath + ".exe"))
                return spath + ".exe";
            
            // TODO: .so, .c, .cs, .luac
            
            return spath; // let the caller handle the invalid filename
        }

        
        [STAThread]
        static void Main(string[] args)
        {
            // Create global variables
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GlobalEnvironment = LuaRuntime.CreateGlobalEnviroment();
            
            // how to handle errors
            GlobalEnvironment.SetNameValue("DEBUG", LuaBoolean.True);
            //GlobalEnvironment.SetNameValue("DEBUG", LuaBoolean.False);
            
            Prompt = "> ";
            
            // check command line args
            if (args.Length > 0)
            {
                string file = args[0];
                if (File.Exists(file))
                {
                    try
                    {
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
            
            // load startup scripts
            LoadFiles();
            
            if (GoInteractive)
            {
                PrintBanner();
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
                            LuaRuntime.Run(line, GlobalEnvironment);
                        }
                        catch (Exception error)
                        {
                            if (((LuaBoolean)GlobalEnvironment.GetValue(GlobalEnvironment.GetKey("DEBUG"))) == LuaBoolean.True)
                                Console.WriteLine(error.ToString());
                            else
                                Console.WriteLine("Error: " + error.Message);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// private function to check for startup scripts "init", "boot", and "start" of
        /// any valid slua filetype
        /// </summary>
        private static void LoadFiles()
        {
            try {
                // check startup script
                LuaRuntime.RunFile(Application.StartupPath + "\\init", GlobalEnvironment);
            } catch (IOException) {
                // probly dont exist.
                // just ignore
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading startup script 'init': " + e.ToString());
            }
            try {
                // check startup script
                LuaRuntime.RunFile(Application.StartupPath + "\\boot", GlobalEnvironment);
            } catch (IOException) {
                // probly dont exist.
                // just ignore
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading startup script 'boot': " + e.ToString());
            }
            try {
                // check startup script
                LuaRuntime.RunFile(Application.StartupPath + "\\start", GlobalEnvironment);
            } catch (IOException) {
                // probly dont exist.
                // just ignore
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading startup script 'start': " + e.ToString());
            }
        }
        
        public static void PrintBanner()
        {
            Console.WriteLine("SharpLua " + Application.ProductVersion + ", Copyright (C) 2011 mlnlover11 Productions");
            //Console.WriteLine(Prompt);
        }
        
    }
}
