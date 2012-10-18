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
"VARARG"
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
OpcodeType.ABC
};

        // Parameter types (likely to change):
        // Unused/Arbitrary  = 0
        // Register 		 = 1
        // Constant 		 = 2
        // Constant/Register = 3
        // Upvalue 			 = 4
        // Jump Distace 	 = 5
        /*
    local LuaOpcodeParams = {
    ["MOVE"] = {1, 1, 0},
    ["LOADK"] = {1, 2, 0},
    ["LOADBOOL"] = {1, 0, 0},
    ["LOADNIL"] = {1, 1, 1},
    ["GETUPVAL"] = {1, 4, 5},
    ["GETGLOBAL"] = {1, 2, 0},
    ["GETTABLE"] = {1, 1, 3},
    ["SETGLOBAL"] = {1, 2, 0},
    ["SETUPVAL"] = {1, 4, 5},
    ["SETTABLE"] = {1, 3, 3},
    ["NEWTABLE"] = {1, 0, 0},
    ["SELF"] = {1, 1, 3},
    ["ADD"] = {1, 1, 3},
    ["SUB"] = {1, 1, 3},
    ["MUL"] = {1, 1, 3},
    ["DIV"] = {1, 1, 3},
    ["MOD"] = {1, 1, 3},
    ["POW"] = {1, 1, 3},
    ["UNM"] = {1, 1, 0},
    ["NOT"] = {1, 1, 0},
    ["LEN"] = {1, 1, 0},
    ["CONCAT"] = {1, 1, 1},
    ["JMP"] = {0, 5, 0},
    ["EQ"] = {1, 3, 3},
    ["LT"] = {1, 3, 3},
    ["LE"] = {1, 3, 3},
    ["TEST"] = {1, 0, 1},
    ["TESTSET"] = {1, 1, 1},
    ["CALL"] = {1, 0, 0},
    ["TAILCALL"] = {1, 0, 0},
    ["RETURN"] = {1, 0, 0},
    ["FORLOOP"] = {1, 5, 0},
    ["FORPREP"] = {1, 5, 0},
    ["TFORLOOP"] = {1, 5, 0},
    ["SETLIST"] = {1, 0, 0},
    ["CLOSE"] = {1, 0, 0},
    ["CLOSURE"] = {1, 0, 0},
    ["VARARG"] = {1, 1, 0},
    }*/

        public enum LuaOp
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
            VARARG = 37
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

        public LuaOp Opcode
        {
            get
            {
                return (LuaOp)OpcodeNumber;
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
            : this((int)(LuaOp)Enum.Parse(typeof(LuaOp), name, true), num)
        {
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