/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/26/2011
 * Time: 12:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using SharpLua.AST;

namespace SharpLua.Luac
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: luac <file> [outfile]");
                Console.WriteLine("  <file> is the input file name");
                Console.WriteLine("  [outfile] is an optional output file name");
                return 1;
            }
            Parser.Parser p = new SharpLua.Parser.Parser();
            bool success;
            Chunk c = p.ParseChunk(new Parser.TextInput(File.ReadAllText(args[0])), out success);
            if (success)
            {
                string _out = "";
                if (args.Length > 1)
                    _out = args[1];
                else
                    _out = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".sluac";
                SharpLua.Serializer.Serialize(c, _out);
                return 0;
            }
            else
            {
                Console.WriteLine("Parsing error(s)!");
                foreach (Tuple<int, string> t in p.Errors)
                    Console.WriteLine("Line " + t.Item1 + ": " + t.Item2);
            }
            return 1;
        }
    }
}