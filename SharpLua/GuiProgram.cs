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
        
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
                string file = args[0];
                if (File.Exists(file))
                {
                    try
                    {
                        LuaInterpreter.RunFile(file);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }

                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine(file + " not found.");
                }
            }

            GlobalEnvironment = LuaInterpreter.CreateGlobalEnviroment();
            GlobalEnvironment.SetNameValue("showfullerror", LuaBoolean.True);
            
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
                        LuaInterpreter.Run(line, GlobalEnvironment);
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
}