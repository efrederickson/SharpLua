/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/29/2011
 * Time: 5:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.CodeDom.Compiler;
using SharpLua.AST;

namespace SharpLua.Compiler
{
    /// <summary>
    /// A #Lua Compiler
    /// </summary>
    public class Compiler
    {
        public enum OutputType
        {
            Exe,
            Dll,
            //NetModule
            WinFormsExe
        }
        
        public static CompilerResults Compile(string[] filenames, OutputType ot, string outfile)
        {
            //Generate Parameters and Code Provider
            Microsoft.CSharp.CSharpCodeProvider csharp = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerParameters param = new CompilerParameters();
            param.GenerateInMemory = false;
            param.IncludeDebugInformation = true;
            param.GenerateExecutable=ot == OutputType.Dll ? false : true;
            param.OutputAssembly = outfile;
            param.EmbeddedResources.Add("SharpLua.dll");
            param.ReferencedAssemblies.Add("SharpLua.dll");
            string classname2 =(new System.Random(DateTime.Now.Millisecond)).Next().ToString();
            switch (ot)
            {
                case OutputType.Dll:
                    param.CompilerOptions = "/target:library";
                    break;
                case OutputType.Exe:
                    param.CompilerOptions = "/target:exe";
                    break;
                case OutputType.WinFormsExe:
                    param.CompilerOptions = "/target:winexe";
                    break;
                default:
                    param.CompilerOptions = "/target:exe";
                    break;
            }
            param.MainClass = "SharpLua.ClassName" + classname2;
            
            // Generate SharpLua Code
            string SharpLuaScript = GetLSScript();
            SharpLuaScript = SharpLuaScript.Replace("{ClassName}", "ClassName" + classname2);
            string ActualSharpLuaCode ="";
            foreach (string filename in filenames)
            {
                try {
                    ActualSharpLuaCode += System.IO.File.ReadAllText(filename);
                } catch {
                    
                }
                // Attempt basic parsing.
                try
                {
                    Parser.Parser p = new SharpLua.Parser.Parser();
                    bool s;
                    p.ParseChunk(new Parser.TextInput(ActualSharpLuaCode), out s);
                } catch (Exception e) {
                    Console.WriteLine("Parsing Error: " + e.ToString());
                    return null;
                }
                
                // Compiling
                ActualSharpLuaCode = ActualSharpLuaCode.Replace("\\", "\\\\");
                ActualSharpLuaCode = ActualSharpLuaCode.Replace("\"", "\\\"");
                string[] codes  = ActualSharpLuaCode.Split(new string[] {"\n"}, StringSplitOptions.None);
                string newcodes = "";
                for (int i = 0; i < codes.Length; i++)
                    newcodes += "\"" + codes[i].Replace("\n","").Replace("\r","") + "\",";
                
                if (newcodes.EndsWith(","))
                    newcodes = newcodes.Substring(0, newcodes.Length -1);
                SharpLuaScript = SharpLuaScript.Replace("|insertcodehere|", newcodes);
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), System.IO.Path.GetFileNameWithoutExtension(filename) + ".cs"), SharpLuaScript);
            }
            // Compile and return the results
            CompilerResults results= csharp.CompileAssemblyFromSource(param, SharpLuaScript);
            return results;
        }

        private static System.IO.Stream GetEmbeddedStream(string name)
        {
            string embeddedName = String.Format("SharpLua.Compiler.{0}", name);
            var me = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream ret =  me.GetManifestResourceStream(embeddedName);
            if (ret == null)
                throw new Exception("Cannot find '" + embeddedName + "'");
            return ret;
        }
        
        private static string GetLSScript()
        {
            return @"using System;
using System.Reflection;
using System.IO;

namespace SharpLua
{
	/// <summary>
	/// the SharpLua script that is compiled to an exe
	/// </summary>
	public class {ClassName}
	{
		string[] SharpLuaCode = {|insertcodehere|};
				
		[STAThread()]
		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Resolver);
			new {ClassName}().Activate(args);
		}
		
		public void Activate(string[] args)
		{
			try
			{
			    bool verbose = false;
			    foreach (string arg in args)
			    {
			        if (arg == " + "\"/verbose\"" + @")
			            verbose = true;
			    }
				string code = """";
				foreach (string line in SharpLuaCode)
				{
				    code += " + "\"\\n\"" + @" + line;
			    }
				if (verbose)
				    Console.WriteLine(""Code:"" + code);
				SharpLua.LuaRuntime.Run(code);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			//TODO: enable this as a CMD Arg
			//System.Console.WriteLine(" + "\"" + "Press any key to Continue..." + "\"" + @");
			//System.Console.ReadKey(true);
		}
		
    static System.Reflection.Assembly Resolver(object sender, ResolveEventArgs args)
    {
        Assembly a1 = Assembly.GetExecutingAssembly();
        Stream s = a1.GetManifestResourceStream(" + "\"" + "SharpLua.dll" + "\"" + @");
        byte[] block = new byte[s.Length];
        s.Read(block, 0, block.Length);
        Assembly a2 = Assembly.Load(block);
        return a2;
    }
	}
}";
        }
    }
}
