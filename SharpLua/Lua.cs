using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SharpLua.Library;

namespace SharpLua
{
    public class Lua
    {
        public static LuaTable GlobalEnvironment;
        private static bool GoInteractive = false;
        
        [STAThread]
        static void Main(string[] args)
        {
            // Create global variables
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GlobalEnvironment = LuaRuntime.CreateGlobalEnviroment();
            #if DEBUG
            GlobalEnvironment.SetNameValue("showfullerror", LuaBoolean.True);
            #else
            GlobalEnvironment.SetNameValue("showfullerror", LuaBoolean.False);
            #endif
            
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
                if (arg.ToUpper() == "INTERACTIVE") 
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
                    string line = Console.ReadLine();
                    
                    if (line == "quit")
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
                            if (((LuaBoolean)GlobalEnvironment.GetValue(GlobalEnvironment.GetKey("showfullerror"))) == LuaBoolean.True)
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
                LuaRuntime.RunFile(Application.StartupPath + "\\init");
            } catch (Exception) {
                // probly dont exist.
                // just ignore
            }
            try {
                // check startup script
                LuaRuntime.RunFile(Application.StartupPath + "\\boot");
            } catch (Exception) { }
            try {
                // check startup script
                LuaRuntime.RunFile(Application.StartupPath + "\\start");
            } catch (Exception) { }
        }
        
        public static void PrintBanner()
        {
            Console.WriteLine("SharpLua " + Application.ProductVersion + ", Copyright (C) 2011 mlnlover11 Productions");
        }
    }
}