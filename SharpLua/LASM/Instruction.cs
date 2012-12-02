using System;
namespace SharpLua.LASM
{
    public class Instruction
    {
        public static readonly string[] LuaOpName = new string[]
        {
            "MOVE",
            "LOADK",
            "LOADBOOL",
            "LOADNIL",
            "GETUPVAL",
            "GETGLOBAL",
            "GETTABLE",
            "SETGLOBAL",
            "SETUPVAL",
            "SETTABLE",
            "NEWTABLE",
            "SELF",
            "ADD",
            "SUB",
            "MUL",
            "DIV",
            "MOD",
            "POW",
            "UNM",
            "NOT",
            "LEN",
            "CONCAT",
            "JMP",
            "EQ",
            "LT",
            "LE",
            "TEST",
            "TESTSET",
            "CALL",
            "TAILCALL",
            "RETURN",
            "FORLOOP",
            "FORPREP",
            "TFORLOOP",
            "SETLIST",
            "CLOSE",
            "CLOSURE",
            "VARARG",
            "BREAKPOINT",
        };

        public static readonly OpcodeType[] LuaOpTypeLookup = new OpcodeType[]{
            OpcodeType.ABC,
            OpcodeType.ABx,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABx,
            OpcodeType.ABC,
            OpcodeType.ABx,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC, //self = xLEGOx's Question (ABC works)
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.AsBx,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.AsBx,
            OpcodeType.AsBx,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABC,
            OpcodeType.ABx,
            OpcodeType.ABC,
            OpcodeType.ABC,
        };

        // Parameter types (likely to change):
        // Unused/Arbitrary  = 0
        // Register 		 = 1
        // Constant 		 = 2
        // Constant/Register = 3
        // Upvalue 			 = 4
        // Jump Distance 	 = 5

        public enum ParameterType
        {
            Unused = 0,
            Register = 1,
            Consant = 2,
            ConstantOrRegister = 3,
            Upvalue = 4,
            JumpDistance = 5
        }

        public class Params
        {
            public ParameterType A, B, C;

            public Params(int a, int b, int c)
            {
                A = (ParameterType)a;
                B = (ParameterType)b;
                C = (ParameterType)c;
            }
        }

        public static readonly Params[] LuaOpcodeParams = new Params[]
        {
            new Params(1, 1, 0), // move
            new Params(1, 2, 0), // loadk
            new Params(1, 0, 0), // loadbool
            new Params(1, 1, 1), // loadnil
            new Params(1, 4, 5), // getupval
            new Params(1, 2, 0), // getglobal
            new Params(1, 1, 3), // gettable
            new Params(1, 2, 0), // setglobal
            new Params(1, 4, 5), // setupval
            new Params(1, 3, 3), // settable
            new Params(1, 0, 0), // newtable
            new Params(1, 1, 3), // self
            new Params(1, 1, 3), // add
            new Params(1, 1, 3), // sub
            new Params(1, 1, 3), // mul
            new Params(1, 1, 3), // div
            new Params(1, 1, 3), // mod
            new Params(1, 1, 3), // pow
            new Params(1, 1, 0), // unm
            new Params(1, 1, 0), // not
            new Params(1, 1, 0), // len
            new Params(1, 1, 1), // concat
            new Params(0, 5, 0), // jmp
            new Params(1, 3, 3), // eq
            new Params(1, 3, 3), // lt
            new Params(1, 3, 3), // le
            new Params(1, 0, 1), // test
            new Params(1, 1, 1), // testset
            new Params(1, 0, 0), // call
            new Params(1, 0, 1), // test
            new Params(1, 0, 0), // tailcall
            new Params(1, 0, 0), // return
            new Params(1, 5, 0), // forloop
            new Params(1, 5, 0), // forprep
            new Params(1, 5, 0), // tforloop
            new Params(1, 0, 0), // setlist
            new Params(1, 0, 0), // close
            new Params(1, 0, 0), // closure
            new Params(1, 1, 0), // vararg
            new Params(0, 0, 0), // breakpoint
        };

        public enum LuaOpcode
        {
            MOVE = 0,
            LOADK = 1,
            LOADBOOL = 2,
            LOADNIL = 3,
            GETUPVAL = 4,
            GETGLOBAL = 5,
            GETTABLE = 6,
            SETGLOBAL = 7,
            SETUPVAL = 8,
            SETTABLE = 9,
            NEWTABLE = 10,
            SELF = 11,
            ADD = 12,
            SUB = 13,
            MUL = 14,
            DIV = 15,
            MOD = 16,
            POW = 17,
            UNM = 18,
            NOT = 19,
            LEN = 20,
            CONCAT = 21,
            JMP = 22,
            EQ = 23,
            LT = 24,
            LE = 25,
            TEST = 26,
            TESTSET = 27,
            CALL = 28,
            TAILCALL = 29,
            RETURN = 30,
            FORLOOP = 31,
            FORPREP = 32,
            TFORLOOP = 33,
            SETLIST = 34,
            CLOSE = 35,
            CLOSURE = 36,
            VARARG = 37,
            BREAKPOINT = 38,
        }

        public long A, B, C, Bx, sBx;
        public string OpcodeName
        {
            get
            {
                return LuaOpName[OpcodeNumber];
            }
        }
        public int OpcodeNumber;

        public LuaOpcode Opcode
        {
            get
            {
                return (LuaOpcode)OpcodeNumber;
            }
        }

        public OpcodeType OpcodeType
        {
            get
            {
                return LuaOpTypeLookup[OpcodeNumber];
            }
        }

        public int Number, LineNumber;

        public Params OpcodeParams
        {
            get
            {
                return LuaOpcodeParams[OpcodeNumber];
            }
        }

        public Instruction(int opcode, int num = 0)
        {
            A = 0;
            B = 0;
            C = 0;
            Bx = 0;
            sBx = 0;
            //Opcode = LuaOpName[opcode];
            OpcodeNumber = opcode;// -1;
            //OpcodeType = LuaOpTypeLookup[opcode];
            //OpcodeParams = LuaOpcodeParams[LuaOpName[opcode]];
            Number = num;
            LineNumber = 0;
        }
        public Instruction(string name, int num = 0)
            : this((int)(LuaOpcode)Enum.Parse(typeof(LuaOpcode), name, true), num)
        {
        }

        public static Instruction From(uint binary)
        {
            int opcode = (int)Lua.GET_OPCODE(binary);
            //(int)Bit.Get(op, 1, 6);
            Instruction instr = new Instruction(opcode, 0);
            if (instr.OpcodeType == OpcodeType.ABC)
            {
                instr.A = Lua.GETARG_A(binary);
                instr.B = Lua.GETARG_B(binary);
                instr.C = Lua.GETARG_C(binary);
                //instr.A = Bit.Get(op, 7, 14);
                //instr.B = Bit.Get(op, 24, 32);
                //instr.C = Bit.Get(op, 15, 23);
            }
            else if (instr.OpcodeType == OpcodeType.ABx)
            {
                instr.A = Lua.GETARG_A(binary);
                instr.Bx = Lua.GETARG_Bx(binary);
                //instr.A = Bit.Get(op, 7, 14);
                //instr.Bx = Bit.Get(op, 15, 32);
            }
            else if (instr.OpcodeType == OpcodeType.AsBx)
            {
                instr.A = Lua.GETARG_A(binary);
                instr.sBx = Lua.GETARG_sBx(binary);
                //instr.A = Bit.Get(op, 7, 14);
                //instr.sBx = Bit.Get(op, 15, 32) - 131071;
            }
            return instr;
        }
    }

    public class Local
    {
        public string Name;
        public int StartPC, EndPC;

        public Local(string name, int s, int e)
        {
            this.Name = name;
            this.StartPC = s;
            this.EndPC = e;
        }
    }

    public class Constant
    {
        public ConstantType Type;
        public object Value = null;
        public int Number = 0;
        public Constant(ConstantType t, object val)
        {
            Type = t;
            Value = val;
        }
    }

    public class Upvalue
    {
        public string Name;

        public Upvalue(string name)
        {
            Name = name;
        }
    }
}