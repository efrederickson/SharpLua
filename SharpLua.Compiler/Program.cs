/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/29/2011
 * Time: 5:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.CodeDom.Compiler;
using System.IO;

namespace SharpLua.Compiler
{
    class Program
    {
        public static int Main(string[] args)
        {
            string inFile, outFile;
            Compiler.OutputType ot;
            if (args.Length == 1)
            {
                inFile = args[0];
                outFile = Path.GetDirectoryName(inFile) + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".exe";
                ot = Compiler.OutputType.Exe;
            }//TODO: check for -out:winexe|exe
            if (args.Length == 2)
            {
                inFile = args[0];
                outFile = args.Length > 1 ? args[1] : Path.GetDirectoryName(inFile) + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".exe";
                ot = Compiler.OutputType.Exe;
            }
            if (args.Length == 3)
            {
                inFile = args[0];
                outFile = args.Length > 1 ? args[1] : Path.GetDirectoryName(inFile) + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".exe";
                ot = Compiler.OutputType.Exe;
            }
            
            Console.WriteLine("Compiling '" + inFile + "'...");
            // compile
            CompilerResults cr = Compiler.Compile(new string[] { inFile }, ot, outFile);
            // display errors
            if (cr.Errors.Count > 0)
            {
                foreach (CompilerError err in cr.Errors)
                    Console.WriteLine("Compile Error: " + err);
                return 1;
            }
            else
            {
                Console.WriteLine("Compiled to '" + outFile + "'!");
            }
            // return
            return 0;
        }
    }
}