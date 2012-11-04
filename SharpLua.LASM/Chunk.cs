using System;
using System.Collections.Generic;
namespace SharpLua.LASM
{
	/// <summary>
	/// A Function/Proto/Chunk
	/// </summary>
	public class Chunk
	{
		/// <summary>
		/// The Chunk's name
		/// </summary>
		public string Name = "";
		/// <summary>
		/// The first line
		/// </summary>
		public uint FirstLine = 1;
		/// <summary>
		/// The last line
		/// </summary>
		public ulong LastLine = 1;
		/// <summary>
		/// The number of upvalues in the chunk
		/// </summary>
		public int UpvalueCount = 0;
		/// <summary>
		/// The number of arguments this function/chunk recieves
		/// </summary>
		public int ArgumentCount = 0;
		/// <summary>
		/// The vararg flag of the chunk
		/// </summary>
		public int Vararg = 0;
		/// <summary>
		/// The Maximum stack size for this chunk
		/// </summary>
		public uint MaxStackSize = 10;
		/// <summary>
		/// The instructions in this chunk
		/// </summary>
		public List<Instruction> Instructions = new List<Instruction>();
		/// <summary>
		/// The constants in this chunk
		/// </summary>
		public List<Constant> Constants = new List<Constant>();
		/// <summary>
		/// The functions/protos in the chunk
		/// </summary>
		public List<Chunk> Protos = new List<Chunk>();
		/// <summary>
		/// The locals in the chunk
		/// </summary>
		public List<Local> Locals = new List<Local>();
		/// <summary>
		/// The upvalues in the chunk
		/// </summary>
		public List<Upvalue> Upvalues = new List<Upvalue>();

		/// <summary>
		/// Creates a new empty chunk
		/// </summary>
		public Chunk() { }

		/// <summary>
		/// Creates a chunk from a Lua proto
		/// </summary>
		/// <param name="p"></param>
		public Chunk(Lua.Proto p)
		{
			Name = p.source.str.ToString();
			MaxStackSize = p.maxstacksize;
			Vararg = p.is_vararg;
			ArgumentCount = p.numparams;
			UpvalueCount = p.nups;
			LastLine = (ulong)p.lastlinedefined;
			FirstLine = (uint)p.linedefined;

			foreach (uint instr in p.code)
				Instructions.Add(Instruction.From(instr));

			foreach (Lua.lua_TValue k in p.k)
			{
				if (k.tt == Lua.LUA_TNIL)
					Constants.Add(new Constant(ConstantType.Nil, null));
				else if (k.tt == Lua.LUA_TBOOLEAN)
					Constants.Add(new Constant(ConstantType.Bool, (int)k.value.p != 0));
				else if (k.tt == Lua.LUA_TNUMBER)
					Constants.Add(new Constant(ConstantType.Number, k.value.n));
				else if (k.tt == Lua.LUA_TSTRING)
					Constants.Add(new Constant(ConstantType.String, k.value.p.ToString()));
			}

			if (p.protos != null)
				foreach (Lua.Proto p2 in p.protos)
					Protos.Add(new Chunk(p2));

			foreach (Lua.LocVar l in p.locvars)
				Locals.Add(new Local(l.varname.str.ToString(), l.startpc, l.endpc));

			foreach (Lua.TString upval in p.upvalues)
				Upvalues.Add(new Upvalue(upval.str.ToString()));

			for (int i = 0; i < p.lineinfo.Length; i++)
				Instructions[i].LineNumber = p.lineinfo[i];


		}

		public string Compile(LuaFile file)
		{
			Func<double, string> DumpNumber = PlatformConfig.GetNumberTypeConvertTo(file);

			Func<int, string> DumpInt = new Func<int, string>(delegate(int num)
			                                                  {
			                                                  	string v = "";
			                                                  	for (int i = 0; i < file.IntegerSize; i++)
			                                                  	{
			                                                  		v += (char)(num % 256);
			                                                  		num = (int)Math.Floor((double)num / 256);
			                                                  	}
			                                                  	return v;
			                                                  });

			Func<string, string> DumpString = new Func<string, string>(delegate(string s)
			                                                           {
			                                                           	int len = file.SizeT;
			                                                           	if (s == null || s.Length == 0)
			                                                           		return "\0".Repeat(len);
			                                                           	else
			                                                           	{
			                                                           		string l = DumpInt(s.Length + 1);
			                                                           		return l + s + "\0";
			                                                           	}
			                                                           });

			string c = "";
			c += DumpString(Name);
			c += DumpInt((int)FirstLine);
			c += DumpInt((int)LastLine);
			c += (char)UpvalueCount;
			c += (char)ArgumentCount;
			c += (char)Vararg;
			c += (char)MaxStackSize;

			// Instructions
			c += DumpInt(Instructions.Count);
			foreach (Instruction i in Instructions)
				c += DumpBinary.Opcode(i);


			// Constants
			c += DumpInt(Constants.Count);
			foreach (Constant cnst in Constants)
			{
				if (cnst.Type == ConstantType.Nil)
					c += (char)0;
				else if (cnst.Type == ConstantType.Bool)
				{
					c += (char)1;
					c += (char)((bool)cnst.Value ? 1 : 0);
				}
				else if (cnst.Type == ConstantType.Number)
				{
					c += (char)3;
					c += DumpNumber((double)cnst.Value);
				}
				else if (cnst.Type == ConstantType.String)
				{
					c += (char)4;
					c += DumpString((string)cnst.Value);
				}
				else
					throw new Exception("Invalid constant type: " + cnst.Type.ToString());
			}

			// Protos
			c += DumpInt(Protos.Count);
			foreach (Chunk ch in Protos)
				c += ch.Compile(file);


			// Line Numbers
			int ln = 0;
			for (int i = 0; i < Instructions.Count; i++)
				if (Instructions[i].LineNumber != 0)
					ln = i;

			c += DumpInt(ln);
			for (int i = 0; i < ln; i++)
				c += DumpInt(Instructions[i].LineNumber);

			//c += DumpInt(Instructions.Count);
			//foreach (Instruction i in Instructions)
			//    c += DumpInt(i.LineNumber);


			// Locals
			c += DumpInt(Locals.Count);
			foreach (Local l in Locals)
			{
				c += DumpString(l.Name);
				c += DumpInt(l.StartPC);
				c += DumpInt(l.EndPC);
			}

			// Upvalues
			c += DumpInt(Upvalues.Count);
			foreach (Upvalue v in Upvalues)
				c += DumpString(v.Name);
			return c;
		}

		public void StripDebugInfo()
		{
			Name = "";
			FirstLine = 1;
			LastLine = 1;
			foreach (Instruction i in Instructions)
				i.LineNumber = 0;
			Locals.Clear();
			foreach (Chunk p in Protos)
				p.StripDebugInfo();
			Upvalues.Clear();
		}

		public void Verify()
		{
			Verifier.VerifyChunk(this);
			foreach (Chunk c in Protos)
				c.Verify();
		}
	}
}