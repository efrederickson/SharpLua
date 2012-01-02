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
            string inFile = null, outFile = null;
            Compiler.OutputType ot = Compiler.OutputType.Exe;
            if (args.Length > 1)
            {
                foreach (string a in args)
                {
                    if (a.ToLower().StartsWith("-ot:"))
                    {
                        string b = a.Substring("-ot:".Length);
                        if (b.ToLower() == "winexe")
                            ot = Compiler.OutputType.WinFormsExe;
                        else
                            ot = Compiler.OutputType.Exe;
                    }
                    if (a.ToLower().StartsWith("-out:"))
                    {
                        string b = a.Substring("-out:".Length);
                        outFile = b;
                    }
                    else
                    {
                        inFile = a;
                    }
                }
                if (inFile == null)
                {
                    Console.WriteLine("Error: Input file not specified!");
                    return 1;
                }
                if (outFile == null)
                    outFile = Path.GetDirectoryName(inFile) + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".exe";
                
            }
            else
            {
                inFile = args[0];
                outFile = Path.GetDirectoryName(inFile) + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".exe";
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