using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

using SharpLua.AST;
using SharpLua.Library;
using SharpLua.LuaTypes;

namespace SharpLua
{
    /// <summary>
    /// A Lua Runtime for running files, strings, and has a REPL
    /// </summary>
    public class LuaRuntime
    {
        /// <summary>
        /// The Prompt used in Interactive Mode
        /// </summary>
        public static string Prompt
        {get; set; }
        
        /// <summary>
        /// The GlobalEnvironment used by #Lua when calling CreateGlobalEnvironment
        /// It also is used in many of the base #Lua functions
        /// </summary>
        public static LuaTable GlobalEnvironment;
        
        private static bool GoInteractive = false;
        
        /// <summary>
        /// Runs a file using a new environment
        /// </summary>
        /// <param name="luaFile"></param>
        /// <returns></returns>
        public static LuaValue RunFile(string luaFile)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile));
        }
        
        /// <summary>
        /// Runs a file using the given environment (useful for using in loading files
        /// into a session)
        /// </summary>
        /// <param name="luaFile"></param>
        /// <param name="enviroment"></param>
        /// <returns></returns>
        public static LuaValue RunFile(string luaFile, LuaTable enviroment)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile), enviroment);
        }
        
        /// <summary>
        /// Runs given code in a new environment
        /// </summary>
        /// <param name="luaCode"></param>
        /// <returns></returns>
        public static LuaValue Run(string luaCode)
        {
            return Run(luaCode, CreateGlobalEnviroment());
        }
        
        /// <summary>
        /// Runs code in the given environment
        /// </summary>
        /// <param name="luaCode"></param>
        /// <param name="enviroment"></param>
        /// <returns></returns>
        public static LuaValue Run(string luaCode, LuaTable enviroment)
        {
            Chunk chunk = Parse(luaCode);
            chunk.Enviroment = enviroment;
            return chunk.Execute();
        }
        
        static Parser.Parser parser = new Parser.Parser();
        
        /// <summary>
        /// Parses some code, returns the parsed code
        /// </summary>
        /// <param name="luaCode"></param>
        /// <returns></returns>
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
                Parser.ParserException ex = new SharpLua.Parser.ParserException(parser.Errors, "Code has syntax errors:\r\n" + parser.GetErrorMessages());
                throw ex;
            }
        }
        
        /// <summary>
        /// Creates a global environment with all the base modules registered and
        /// some default values set.
        /// </summary>
        /// <returns></returns>
        public static LuaTable CreateGlobalEnviroment(bool createBaseLib = true,
                                                      bool createStringLib = true,
                                                      bool createTableLib = true,
                                                      bool createOSLib = true,
                                                      bool createIOLib = true,
                                                      bool createFileLib = true,
                                                      bool createMathLib = true,
                                                      bool createScriptLib = true,
                                                      bool createWinFormsLib = true,
                                                      bool createConsoleLib = true,
                                                      bool createCoroutineLib = true,
                                                      bool createPackageLib = true,
                                                      bool createClassLib = true,
                                                      bool createFileSystemLib = true)
        {
            LuaTable global = new LuaTable();
            
            // Register Lua Modules
            
            if (createBaseLib)
                BaseLib.RegisterFunctions(global);
            if (createStringLib)
                StringLib.RegisterModule(global);
            if (createTableLib)
                TableLib.RegisterModule(global);
            if (createIOLib)
                IOLib.RegisterModule(global);
            if (createFileLib)
                FileLib.RegisterModule(global);
            if (createMathLib)
                MathLib.RegisterModule(global);
            if (createOSLib)
                OSLib.RegisterModule(global);
            if (createScriptLib)
                ScriptLib.RegisterModule(global);
            if (createWinFormsLib)
                WinFormLib.RegisterModule(global);
            if (createConsoleLib)
                ConsoleLib.RegisterModule(global);
            if (createCoroutineLib)
                CoroutineLib.RegisterModule(global);
            if (createPackageLib)
                PackageLib.RegisterModule(global);
            if (createClassLib)
                ClassLib.RegisterModule(global);
            if (createFileSystemLib)
                FileSystemLib.RegisterModule(global);
            
            global.SetNameValue("_WORKDIR", new LuaString(Application.StartupPath + "\\"));
            global.SetNameValue("_VERSION", new LuaString("Sharp Lua 1.1"));
            global.SetNameValue("_G", global);
            
            if (createPackageLib)
            {
                // set package.preload table
                LuaTable preload = (LuaTable) (global.GetValue("package") as LuaTable).GetValue("preload");
                if (createStringLib)
                    preload.SetNameValue("string", (LuaTable) global.GetValue("string"));
                if (createTableLib)
                    preload.SetNameValue("table", (LuaTable) global.GetValue("table"));
                if (createIOLib)
                    preload.SetNameValue("io", (LuaTable) global.GetValue("io"));
                if (createFileLib)
                    preload.SetNameValue("file", (LuaTable) global.GetValue("file"));
                if (createMathLib)
                    preload.SetNameValue("math", (LuaTable) global.GetValue("math"));
                if (createOSLib)
                    preload.SetNameValue("os", (LuaTable) global.GetValue("os"));
                if (createScriptLib)
                    preload.SetNameValue("script", (LuaTable) global.GetValue("script"));
                if (createWinFormsLib)
                    preload.SetNameValue("WinForms", (LuaTable) global.GetValue("WinForms"));
                if (createConsoleLib)
                    preload.SetNameValue("console", (LuaTable) global.GetValue("console"));
                if (createCoroutineLib)
                    preload.SetNameValue("coroutine", (LuaTable) global.GetValue("coroutine"));
                if (createPackageLib) // wait a second...
                    preload.SetNameValue("package", (LuaTable) global.GetValue("package"));
                if (createClassLib)
                    preload.SetNameValue("class", (LuaTable) global.GetValue("class"));
                if (createFileSystemLib)
                    preload.SetNameValue("filesystem", (LuaTable) global.GetValue("filesystem"));
            }
            if (createFileSystemLib)
                FileSystemLib.currentDir = global.GetValue("_WORKDIR").ToString();
            
            GlobalEnvironment = global;
            return global;
        }
        
        /// <summary>
        /// Attempts to find a file from a shortened path (no extension)
        /// </summary>
        /// <param name="spath"></param>
        /// <returns></returns>
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
            
            // TODO: .cs, .luac; possibly C
            
            return spath; // let the caller handle the invalid filename
        }
        
        /// <summary>
        /// A REPL (Read, Eval, Print, Loop function)
        /// </summary>
        /// <param name="args">Application startup args</param>
        public static void REPL(string[] args)
        {
            // Create global variables
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GlobalEnvironment = LuaRuntime.CreateGlobalEnviroment();
            PrintBanner();
            // how to handle errors
            #if DEBUG
            GlobalEnvironment.SetNameValue("DEBUG", LuaBoolean.True);
            #else
            GlobalEnvironment.SetNameValue("DEBUG", LuaBoolean.False);
            #endif
            
            Prompt = "> ";
            // load startup scripts
            LoadFiles();
            
            // check command line args
            if (args.Length > 0)
            {
                string file = args[0];
                if (File.Exists(file))
                {
                    try
                    {
                        GlobalEnvironment.SetNameValue("_WORKDIR", new LuaString(Path.GetDirectoryName(file)));
                        if (file.EndsWith(".out") || file.EndsWith(".luac") || file.EndsWith(".sluac"))
                        {
                            Chunk c = Serializer.Deserialize(file) as Chunk;
                            c.Enviroment = GlobalEnvironment;
                            bool isBreak;
                            c.Execute(GlobalEnvironment, out isBreak).ToString();
                        }
                        else
                        {
                            LuaRuntime.RunFile(file, GlobalEnvironment);
                        }
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
                            LuaValue v = Run(line, GlobalEnvironment);
                            if (v == null)
                                Console.WriteLine("=> [no returned value]");
                            else
                                Console.WriteLine("=> " + v.ToString());
                        }
                        catch (Parser.ParserException error)
                        {
                            // create spacing
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < Prompt.Length; i++)
                                sb.Append(" ");
                            for (int i = 0; i < error.FirstErrorIndex; i++)
                                sb.Append(" ");
                            
                            Console.WriteLine(sb.ToString() + "^");
                            Console.WriteLine("Error: " + error.Message);
                            GlobalEnvironment.SetNameValue("LASTERROR", ObjectToLua.ToLuaValue(error));
                        }
                        catch (Exception error)
                        {
                            if (((LuaBoolean)GlobalEnvironment.GetValue(GlobalEnvironment.GetKey("DEBUG"))) == LuaBoolean.True)
                                Console.WriteLine(error.ToString());
                            else
                                Console.WriteLine("Error: " + error.Message);
                            GlobalEnvironment.SetNameValue("LASTERROR", ObjectToLua.ToLuaValue(error));
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
        
        /// <summary>
        /// Prints the SharpLua Banner
        /// </summary>
        public static void PrintBanner()
        {
            Console.WriteLine("SharpLua " + AssemblyVersion.Version + ", Copyright (C) 2011-2012 mlnlover11 Productions");
            //Console.WriteLine(Prompt);
        }
        
    }
}
