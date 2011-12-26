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

namespace SharpLua.Compiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            Parser.Parser p = new SharpLua.Parser.Parser();
            bool success;
            Chunk c = p.ParseChunk(new Parser.TextInput(File.ReadAllText(args[0])), out success);
            if (success)
            {
                string _out = "";
                if (args.Length > 1)
                    _out = args[1];
                else
                    _out = Path.GetFileNameWithoutExtension(args[0]) + ".sluac";
                Serializer.Serialize(c, _out);
            }
            else
            {
                Console.WriteLine("Parsing error(s)!");
                foreach (Tuple<int, string> t in p.Errors)
                    Console.WriteLine("Line " + t.Item1 + ": " + t.Item2);
            }
        }
    }
}