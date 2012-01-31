/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 1/14/2012
 * Time: 8:55 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SharpLua.Launcher
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            SharpLua.LuaRuntime.REPL(args);
        }
        
    }
}
