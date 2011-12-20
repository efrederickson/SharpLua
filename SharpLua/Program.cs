using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Language.Lua
{
    class Program
    {
        static void Main(string[] args)
        {
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

            LuaTable global = LuaInterpreter.CreateGlobalEnviroment();

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
                        LuaInterpreter.Interpreter(line, global);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                }
            }
        }
    }
}
