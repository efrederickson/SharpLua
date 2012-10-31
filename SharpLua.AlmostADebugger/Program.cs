using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SharpLua.AlmostADebugger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Console.WriteLine("Initializing GUI...");
            LuaRuntime.Run(System.IO.File.ReadAllText("Core.lua"));
            Application.Run(new Form1());
            Console.WriteLine("Exiting...");
        }
    }
}
