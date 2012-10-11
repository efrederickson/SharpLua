local InstructionTable = {
{Name = 'MOVE'; Opcode = 0; Format = 'ABC'; };
{Name = 'LOADK'; Opcode = 1; Format = 'ABx'; };
{Name = 'LOADBOOL'; Opcode = 2; Format = 'ABC'; };
{Name = 'LOADNIL'; Opcode = 3; Format = 'ABC'; };
{Name = 'GETUPVAL'; Opcode = 4; Format = 'ABC'; };
{Name = 'GETGLOBAL'; Opcode = 5; Format = 'ABx'; };
{Name = 'GETTABLE'; Opcode = 6; Format = 'ABC'; };
{Name = 'SETGLOBAL'; Opcode = 7; Format = 'ABx'; };
{Name = 'SETUPVAL'; Opcode = 8; Format = 'ABC'; };
{Name = 'SETTABLE'; Opcode = 9; Format = 'ABC'; };

{Name = 'NEWTABLE'; Opcode = 10; Format = 'ABC'; };
{Name = 'SELF'; Opcode = 11; Format = 'ABC'; };
{Name = 'ADD'; Opcode = 12; Format = 'ABC'; };
{Name = 'SUB'; Opcode = 13; Format = 'ABC'; };
{Name = 'MUL'; Opcode = 14; Format = 'ABC'; };
{Name = 'DIV'; Opcode = 15; Format = 'ABC'; };
{Name = 'MOD'; Opcode = 16; Format = 'ABC'; };
{Name = 'POW'; Opcode = 17; Format = 'ABC'; };
{Name = 'UNM'; Opcode = 18; Format = 'ABC'; };
{Name = 'NOT'; Opcode = 19; Format = 'ABC'; };

{Name = 'LEN'; Opcode = 20; Format = 'ABC'; };
{Name = 'CONCAT'; Opcode = 21; Format = 'ABC'; };
{Name = 'JMP'; Opcode = 22; Format = 'AsBx';};
{Name = 'EQ'; Opcode = 23; Format = 'ABC'; };
{Name = 'LT'; Opcode = 24; Format = 'ABC'; };
{Name = 'LE'; Opcode = 25; Format = 'ABC'; };
{Name = 'TEST'; Opcode = 26; Format = 'ABC'; };
{Name = 'TESTSET'; Opcode = 27; Format = 'ABC'; };
{Name = 'CALL'; Opcode = 28; Format = 'ABC'; };
{Name = 'TAILCALL'; Opcode = 29; Format = 'ABC'; };

{Name = 'RETURN'; Opcode = 30; Format = 'ABC'; };
{Name = 'FORLOOP'; Opcode = 31; Format = 'AsBx';};
{Name = 'FORPREP'; Opcode = 32; Format = 'AsBx';};
{Name = 'TFORLOOP'; Opcode = 33; Format = 'ABC'; };

{Name = 'SETLIST'; Opcode = 34; Format = 'ABC'; };
{Name = 'CLOSE'; Opcode = 35; Format = 'ABC'; };
{Name = 'CLOSURE'; Opcode = 36; Format = 'ABx'; };
{Name = 'VARARG'; Opcode = 37; Format = 'ABC'; };
}

local InstructionsByOpcode = {}
local InstructionsByName = {}
for _, inst in pairs(InstructionTable) do
InstructionsByOpcode[inst.Opcode] = inst
InstructionsByName[inst.Name] = inst
end


bit = {
get = function(num, n, n2)
if n2 then
local total = 0
local digitn = 0
for i = n, n2 do
total = total + 2^digitn*bit.get(num, i)
digitn = digitn + 1
end
return total
else
local pn = 2^(n-1)
return (num % (pn + pn) >= pn) and 1 or 0 
end
end;
}

function DisassembleChunk(chunk)
local ptr = 1

--general getter functions
local function GetInt8()
local byte = chunk:sub(ptr, ptr):byte()
ptr = ptr + 1
return byte
end
local function GetInt16()
local a = GetInt8()
local b = GetInt8()
return 256*b + a
end
local function GetInt32()
local a = GetInt8()
local b = GetInt8()
local c = GetInt8()
local d = GetInt8()
return d*16777216 + c*65536 + b*256 + a
end
local function GetInt64()
local a = GetInt8()
local b = GetInt8()
local c = GetInt8()
local d = GetInt8()
local e = GetInt8()
local _, _, _ = GetInt8(), GetInt8(), GetInt8()
return e*4294967296 + d*16777216 + c*65536 + b*256 + a
end
local function GetFloat64()
local a = GetInt32()
local b = GetInt32()
return (-2*bit.get(b, 32)+1)*(2^(bit.get(b, 21, 31)-1023))*((bit.get(b, 1, 20)*(2^32) + a)/(2^52)+1)
end
local function GetString(len)
local str = chunk:sub(ptr, ptr+len-1)
ptr = ptr + len
return str
end
local GetIntSz = {
[1] = GetInt8;
[2] = GetInt16;
[4] = GetInt32;
[8] = GetInt64;
}


--read in various parts of the chunk
local GetData_sizet;
local GetData_Int;
local GetData_Instruction = GetInt32;
local function GetData_String()
local length = GetData_sizet()
return GetString(length):sub(1,-2)
end
local function GetData_Proto()
local chunkName = GetData_String()
local definitionStart = GetData_Int()
local definitionEnd = GetData_Int()
local upvals = GetInt8()
local params = GetInt8()
local varargFlag = GetInt8()
local maxStack = GetInt8()
--
print(chunkName:sub(1,1):byte())
print(string.format("ChunkName: %s\n"..
"Defined at lines %d to %d\n"..
"Upvalues: %d\n"..
"Parameters: %d\n"..
"VarargFlag: 0x%x\n"..
"MaxStack: %d\n", 
chunkName, definitionStart, definitionEnd, upvals,
params, varargFlag, maxStack))

--instruction list
local instructions = {}
local instructionCount = GetData_Int()
for i = 1, instructionCount do
instructions[i] = GetData_Instruction()
end

--constant list
local constants = {}
local constantCount = GetData_Int()
for i = 1, constantCount do
local t = GetInt8()
if t == 0 then
constants[i] = nil
elseif t == 1 then
constants[i] = (GetInt8() == 1)
elseif t == 3 then
constants[i] = GetFloat64()
elseif t == 4 then
constants[i] = GetData_String()
end
--print("K("..(i-1)..") = "..tostring(constants[i]))
end

--proto list
local protos = {}
local protoCount = GetData_Int()
for i = 1, protoCount do
protos[i] = GetData_Proto()
end

--source line position
local sourceLines = {}
local sourceLineCount = GetData_Int()
for i = 1, sourceLineCount do
sourceLines[i] = GetData_Int()
end

--locals
local localVars = {}
local localVarCount = GetData_Int()
for i = 1, localVarCount do
local var = {}
var.Name = GetData_String()
var.Start = GetData_Int()
var.End = GetData_Int()
localVars[i] = var
end

--upvalues
local upvalues = {}
local upvalueCount = GetData_Int()
for i = 1, upvalueCount do
upvalues[i] = GetData_String()
end

--translate instructions
local function K(v)
local v = constants[v+1]
if type(v) == 'string' then
return '"'..v..'"'
else
return tostring(v)
end
end
local function RorK(v)
if v > 255 then
return K(v-256)
else
return "R"..v
end
end
local BinopToSymbol = {
ADD = '+';
SUB = '-';
MUL = '*';
DIV = '/';
MOD = '%';
POW = '^';
LEN = '#';
}
local UnopToSymbol = {
NOT = 'not';
UNM = '-';
}
--=========================================--
local function MkReg(n)
return {
type = 'Reg';
index = n;
}
end
local function MkConst(n)
return {
type = 'Const';
value = constants[n+1];
}
end
local function MkRegOrConst(v)
if v >= 256 then
return MkConst(v-256)
else
return MkReg(v)
end
end
--=========================================--
for i = 1, instructionCount do
local instCode = instructions[i]
local opcode = (instCode % 2^6)
local inst = InstructionsByOpcode[opcode]
--
local instruction = {}
instruction.Instruction = inst
instruction.Name = inst.Name
instruction.Location = i
instruction.Read = {}
instruction.Write = {}
instruction.JumpTo = {}
instruction.JumpTargetOf = {}
instructions[i] = instruction
--
if inst.Format == 'ABC' then
local a = math.floor((instCode % 2^(6+8) )/2^(6) )
local c = math.floor((instCode % 2^(6+8+9) )/2^(6+8) )
local b = math.floor((instCode % 2^(6+8+9+9))/2^(6+8+9))
instruction.a = a
instruction.b = b
instruction.c = c 

elseif inst.Format == 'ABx' then
local a = math.floor((instCode % 2^(6+8) )/2^(6) )
local bx = math.floor((instCode % 2^(6+8+18)/2^(6+8)))
instruction.a = a
instruction.bx = bx

elseif inst.Format == 'AsBx' then
local a = math.floor((instCode % 2^(6+8) )/2^(6) )
local sbx = math.floor((instCode % 2^(6+8+18)/2^(6+8))) - 131071
instruction.a = a
instruction.sbx = sbx
end
end
for i = 1, instructionCount do
io.write(string.format("%3d: ", i))
if instructions[i] == 'IGNORE' then
print("")
else
local instruction = instructions[i]
--
local function AddRead(dat)
instruction.Read[#instruction.Read+1] = dat
end
local function AddWrite(dat)
instruction.Write[#instruction.Write+1] = dat
end
--
if instruction.Instruction.Format == 'ABC' then
local a = instruction.a
local c = instruction.b
local b = instruction.c
if instruction.Name == 'CALL' then
local callOn, retTo
if b == 0 then
callOn = "R"..(a+1).."+"
else
callOn = ""
for n = a+1, a+b-1 do
callOn = callOn.."R"..n..((n == a+b-1) and "" or ", ")
end
end
if c == 0 then
retTo = "R"..a.."+ = "
elseif c == 1 then
retTo = ""
else
retTo = ""
for n = a, a+c-2 do
retTo = retTo.."R"..n..((n == a+c-2) and "" or ", ")
end
retTo = retTo.." = "
end
print(retTo.."R"..a.."( "..callOn.." )")
-----------------------------------------------------
instruction.Read = {MkReg(a)}
if b == 0 then
local r = MkReg(a+1); r.Vararg = true; AddRead(r)
else
for n = a+1, a+b-1 do AddRead(MkReg(n)) end
end
instruction.Write = {}
if c == 0 then
local r = MkReg(a); r.Vararg = true; AddWrite(r) 
else
for n = a, a+c-2 do AddWrite(MkReg(n)) end
end
-------------------------------------------------------

elseif instruction.Name == 'RETURN' then
local retTo = "return "
if b == 0 then
retTo = "return R"..a.."+"
elseif b == 1 then
retTo = "return"
else
for i = a, a+b-2 do
retTo = retTo.."R"..i..((i == a+b-2) and "" or ", ")
end
end
print(retTo)
--------------------------------------------------------
instruction.Read = {}
if b == 0 then
local r = MkReg(a); r.Vararg = true; AddRead(r)
else
for n = a, a+b-2 do AddRead(MkReg(n)) end
end
instruction.Write = {}
--------------------------------------------------------

elseif instruction.Name == 'MOVE' then
print("R"..a.." = R"..b)
--------------------------------------------------------
instruction.Read = {MkRegOrConst(b)}
instruction.Write = {MkReg(a)}
--------------------------------------------------------

elseif BinopToSymbol[instruction.Name] then
print("R"..a.." = "..RorK(b).." "..BinopToSymbol[inst.Name].." "..RorK(c))
-------------------------------------------------------
instruction.Read = {MkRegOrConst(b), MkRegOrConst(c)}
instruction.Write = {MkReg(a)}
-------------------------------------------------------

elseif UnopToSymbol[instruction.Name] then
print("R"..a.." = "..UnopToSymbol[inst.Name].." "..RorK(b))
-------------------------------------------------------
instruction.Read = {MkRegOrConst(b)}
instruction.Write = {MkReg(a)}
--------------------------------------------------------

elseif instruction.Name == 'GETTABLE' then
print("R"..a.." = R"..b.."[ "..RorK(c).." ]")
--------------------------------------------------------
instruction.Read = {MkReg(b), MkRegOrConst(c)}
instruction.Write = {MkReg(a)}
--------------------------------------------------------

elseif instruction.Name == 'SETTABLE' then
print("R"..a.."[ "..RorK(b).." ] = "..RorK(c))
---------------------------------------------------------
instruction.Read = {MkRegOrConst(b), MkRegOrConst(c)}
instruction.Write = {MkReg(a)}
---------------------------------------------------------

elseif instruction.Name == 'LOADBOOL' then
print("R"..a.." = "..tostring(b > 0))
---------------------------------------------------------
instruction.Instruction = InstructionsByName.MOVE
instruction.Read = {{type = 'Const'; value = (b > 0);}}
instruction.Write = {MkReg(a)}
---------------------------------------------------------

elseif instruction.Name == 'LOADNIL' then
print("R"..a.." = nil")
---------------------------------------------------------
instruction.Instruction = InstructionsByName.MOVE
instruction.Read = {{type = 'Const'; value = nil;}}
instruction.Write = {MkReg(a)}
---------------------------------------------------------

elseif instruction.Name == 'NEWTABLE' then
print("R"..a.." = {}")
---------------------------------------------------------
instruction.Instruction = InstructionsByName.MOVE
instruction.Read = {}
instruction.Write = {MkReg(a)}
---------------------------------------------------------

elseif instruction.Name == 'TESTSET' then
local toGoto;
if instructions[i+1].Instruction == InstructionsByName.JMP then
toGoto = i+1 + instructions[i+1].sbx + 1
instructions[i+1] = 'IGNORE'
end
if toGoto then
print("if"..((c>0) and "" or " not").." R"..b.." then R"..a.." = "..RorK(b)..
"; goto "..toGoto.."; end")
else
print("if"..((c>0) and "" or " not").." R"..b.." then R"..a.." = "..RorK(b)..
"; else PC++; end")
end

--todo: TEST

else
print(instruction.Name, a, b, c) 
end
elseif instruction.Instruction.Format == 'ABx' then
local a = instruction.a
local bx = instruction.bx
if instruction.Name == 'LOADK' then
print("R"..a.." = "..K(bx))
--------------------------------------------------------
instruction.Read = {MkConst(bx)}
instruction.Write = {MkReg(a)}
--------------------------------------------------------

elseif instruction.Name == 'GETGLOBAL' then
print("R"..a.." = _G[ "..K(bx).." ]")
--------------------------------------------------------
instruction.Read = {MkConst(bx)}
instruction.Write = {MkReg(a)}
--------------------------------------------------------

elseif instruction.Name == 'SETGLOBAL' then
print("_G[ "..K(bx).." ] = "..RorK(a))
--------------------------------------------------------
instruction.Read = {MkRegOrConst(a), MkConst(bx)}
instruction.Write = {}
--------------------------------------------------------

else
print(instruction.Name, a, bx)
end


elseif instruction.Instruction.Format == 'AsBx' then
local a = instruction.a
local sbx = instruction.sbx
print(instruction.Name, a, sbx)
end
end
end
end
local function GetData_Header()
local ident = GetString(4)
local version = GetInt8()
local format = GetInt8()
local endian = GetInt8()
local sizeofInt = GetInt8()
local sizeofSizeT = GetInt8()
local sizeofInstruction = GetInt8()
local sizeofNumber = GetInt8()
local numberFormat = GetInt8()
--
assert(sizeofInstruction == 4, "SizeofInstruction must be 4")
assert(sizeofNumber == 8, "SizeofNumber must be 8")
--
assert(GetIntSz[sizeofInt], "Bad SizeofInt: "..sizeofInt)
assert(GetIntSz[sizeofSizeT], "Bad SizeOf size_t: "..sizeofSizeT)
--
GetData_Int = GetIntSz[sizeofInt]
GetData_sizet = GetIntSz[sizeofSizeT]
--
print(ident, version, format, endian, sizeofInt, sizeofSizeT, 
sizeofInstruction, sizeofNumber, numberFormat)
end
------
GetData_Header()
GetData_Proto()
end

DisassembleChunk(string.dump(function(a, b, ...)
print(a or 6)
end))
