/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 1/1/2012
 * Time: 2:29 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SharpLua.Interactive
{
    class Program
    {
        public static void Main(string[] args)
        {
            SharpLua.LuaRuntime.REPL(args);
        }
    }
}