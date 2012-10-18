/*
TODO:
Better error messages

LASM (Lua Assembly) is used to write Lua bytecode.

Controls:
[] - Optional

.const Value
Value = "String", true/false/nil, Number
.local Name     <StartPC, EndPC = 0, 0>
.upval Name
.upvalue Name
.stacksize Value
.maxstacksize value
.vararg <Vararg int value>
.name Name
.options

.func [name]
.function [name]
.end

Opcodes:
<code> <arg> <arg> [<arg>]
The 'C' (3rd) arg is optional, and defaults to 0
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace SharpLua.LASM
{
    public class LasmParser
        {
        LuaFile file;
        int index, lineNumber;
        Chunk func;
        Stack<Chunk> funcStack;

        void parseControl(string line)
        {
            string ll = line.ToLower();
            if (ll.Substring(0, 6) == ".const")
            {
                string l = line.Substring(6);
                l = l.Trim();
                object value = readValue(l);
                if (value == null)
                    func.Constants.Add(new Constant(ConstantType.Nil, null));
                else if (value is bool)
                    func.Constants.Add(new Constant(ConstantType.Bool, (bool)value));
                else if (value is double)
                    func.Constants.Add(new Constant(ConstantType.Number, (double)value));
                else if (value is string)
                    func.Constants.Add(new Constant(ConstantType.String, (string)value));
            }
            else if (ll.Substring(0, 5) == ".name")
            {
                /// Im lazy :P
                string l = line.Substring(5);
                l = l.Trim();
                if (l[0] == '"')
                    func.Name = (string)readValue(l);
                else
                    func.Name = l;
            }
            else if (ll.Substring(0, 8) == ".options")
            {
                string l = line.Substring(8);
                l = l.Trim();
                List<int> nums = new List<int>();
                // Pattern matching time!
                Regex r = new Regex("\\d+");
                foreach (Match m in r.Matches(l))
                    nums.Add(int.Parse(m.Value));
                func.UpvalueCount = nums.Count > 0 ? nums[0] : func.UpvalueCount;
                func.ArgumentCount = nums.Count > 1 ? nums[1] : func.ArgumentCount;
                func.Vararg = nums.Count > 2 ? nums[2] : func.Vararg;
                func.MaxStackSize = nums.Count > 3 ? nums[3] : func.MaxStackSize;
            }
            else if (ll.Substring(0, 6) == ".local")
            {
                string l = line.Substring(6).Trim();
                if (l[0] == '"')
                    func.Locals.Add(new Local((string)readValue(l), 0, 0));
                else
                    func.Locals.Add(new Local(l, 0, 0));
            }
            else if (ll.Substring(0, 6) == ".upval")
            {
                string l = line.Substring(6).Trim();
                if (l[0] == '"')
                    func.Upvalues.Add(new Upvalue((string)readValue(l)));
                else
                    func.Upvalues.Add(new Upvalue(l));
            }
            else if (ll.Substring(0, 8) == ".upvalue")
            {
                string l = line.Substring(8).Trim();
                if (l[0] == '"')
                    func.Upvalues.Add(new Upvalue((string)readValue(l)));
                else
                    func.Upvalues.Add(new Upvalue(l));
            }
            else if (ll.Substring(0, 10) == ".stacksize")
            {
                string l = line.Substring(10).Trim();
                int n = int.Parse(l);
                func.MaxStackSize = n;
            }
            else if (ll.Substring(0, 13) == ".maxstacksize")
            {
                string l = line.Substring(13).Trim();
                int n = int.Parse(l);
                func.MaxStackSize = n;
            }
            else if (ll.Substring(0, 7) == ".vararg")
            {
                string l = line.Substring(7).Trim();
                int n = int.Parse(l);
                func.Vararg = n;
            }
            else if (ll.Substring(0, 9) == ".function")
            {
                string l = line.Substring(9).Trim();
                Chunk n = new Chunk();
                n.FirstLine = (uint)lineNumber;
                if (l.Length > 0)
                    if (l[0] == '"')
                        n.Name = (string)readValue(l);
                    else
                        n.Name = l;
                func.Protos.Add(n);
                funcStack.Push(func);
                func = n;
            }
            else if (ll.Substring(0, 5) == ".func")
            {
                string l = line.Substring(5).Trim();
                Chunk n = new Chunk();
                n.FirstLine = (uint)lineNumber;
                if (l.Length > 0)
                    if (l[0] == '"')
                        n.Name = (string)readValue(l);
                    else
                        n.Name = l;
                func.Protos.Add(n);
                funcStack.Push(func);
                func = n;
            }
            else if (ll.Substring(0, 4) == ".end")
            {
                Chunk f = funcStack.Pop();
                func.LastLine = lineNumber;
                Instruction instr1 = func.Instructions.Count > 0 ? func.Instructions[func.Instructions.Count - 1] : null;
                Instruction instr2 = new Instruction("RETURN");
                instr2.A = 0;
                instr2.B = 1;
                instr2.C = 0;
                if (instr1 != null && instr1.Opcode == Instruction.LuaOp.RETURN)
                { } //func.Instructions.Add(instr2);
                else
                    func.Instructions.Add(instr2);
                    
                func = f;
            }
            else
                throw new Exception("Invalid Control Label");
        }

           int tonumber(string s)
           {
                if (s[0]== '$' 
                    || char.ToLower(s[0])== 'r'
                    || char.ToLower(s[0]) == 'k')
                    s = s.Substring(1);
                return int.Parse(s);
           }

   Instruction parseOpcode(string line)
   {
            string op = "";
            int i = 1;
            string l = line.ToLower();
            while (true)
            {
                if (i > l.Length)
                    break;
                if (!char.IsLetter(l[i]))
                    break;
                else
                    op += l[i++];
            }
            Instruction instr = new Instruction(op, 0);

            line = line.Substring(i).Trim();
            i = 1;

       switch (instr.OpcodeType)
	{
		case OpcodeType.ABC:
               string a = "", b = "", c = "";
                bool inA = true, inB = false;
                while (true)
                {
                    if (line.Length < i)
                        break;

                    char ch = line[i];
                    if (ch == '\t' || ch == ' ')
                        if (inA)
                        {
                            inB = true;
                            inA = false;
                        }
                        else if (inB)
                            inB = false;
                        else
                            break;
                    else
                        if (inA)
                            a = a + ch;
                        else if (inB)
                            b = b + ch;
                        else
                            c = c + ch;
                    i++;
                }
                c = c == "" ? "0" : c;
                instr.A = tonumber(a);
                instr.B = tonumber(b);
                instr.C = tonumber(c);
 break;
case OpcodeType.ABx:
               string bx = "";
               a = "";
               inA = true;
                while (true)
                {
                    if (line.Length < i)
                        break;

                    char ch = line[i];
                    if (ch == '\t' || ch == ' ')
                        if (inA)
                            inA = false;
                        else
                            break;
                    else
                        if (inA)
                            a = a + ch;
                        else
                            bx = bx + ch;
                    i++;
                }
                instr.A = tonumber(a);
                instr.Bx = tonumber(bx);
 break;
case OpcodeType.AsBx:
               string sbx = "";
               inA = true;
                while (true)
                {
                    if (line.Length < i)
                        break;

                    char ch = line[i];
                    if (ch == '\t' || ch == ' ')
                        if (inA)
                            inA = false;
                        else
                            break;
                    else
                        if (inA)
                            a = a + ch;
                        else
                            sbx =sbx + ch;
                    i++;
                }
                instr.A = tonumber(a);
                instr.sBx = tonumber(sbx);
 break;
default:
 break;
	}
            return instr;
   }

    string readVarName()
    {
            local varPattern = "([%w_]*)" -- Any letter, number, or underscore
            local varName = string.match(text, varPattern, index)
            if not varName then
                error("Invalid variable name!")
            end
            index = index + varName:len()
            return varName
    }

    void readComment()
    {
            if (text[index] == ';')
            {
                while (true)
                {
                    if (text.Length < index)
                        break;

                    char c = text[index];
                    if (c == '\r')
                    {
                        index++;
                        if (text[index] == '\n')
                            index++;
                        break;
                    }
                    else if (c == '\n')
                    {
                        index++;
                        break;
                    }
                    else
                    {

                    }
                    index++;
                }
            }
    }

    object readValue(string text)
    {
            int index = 1;
            if (text[index] == '"')
            {
                string s = "";
                index++;
                while (true)
                {
                    char c = text[index];
                    if (c == '\\')
                        {
                        char c2 = text[index + 1];
                        if (c2 == 'n')
                           s += "\n";
                        else if (c2 == 'r')
                            s += "\r";
                        else if (c2 == 't')
                            s += "\t";
                        else if (c2 == '\\')
                            s += "\\";
                        else if (c2 == '"')
                            s +="\"";
                        else if (c2 == '\'')
                            s += "'";
                        else if (c2 == 'a')
                            s += "\a";
                        else if (c2 == 'b')
                            s += "\b";
                        else if (c2 == 'f')
                            s += "\f";
                        else if (c2 == 'v')
                            s += "\v";
                        else if (char.IsNumber(c2))
                        {
                            string ch = text[index + 1].ToString();
                            if (char.IsNumber(text[index + 2]))
                            {
                                index++;
                                ch += text[index + 1];
                                if (char.IsNumber(text[index + 2]))
                                {
                                    index++;
                                    ch += text[index + 2];
                            }
                            }
                            s += int.Parse(ch);
                        }
                        else
                            throw new Exception("Unknown escape sequence: " + c2);
                        index += 2;
                    }
                    else if (c == '"')
                        break;
                    else
                    {
                        index++;
                        s += c;
                    }
                }
                return s;
            }
            else if(text.StartsWith("true"))
                return true;
        else if (text.StartsWith("false"))
                return false;
            else if (text.StartsWith("nil"))
                return null;
            else
            {
                // number
                int x;
                if (!int.TryParse(text.Trim(), out x))
                    throw new Exception("Unable to read value '" + num + "'");
                else
                    return x;
            }
    }

    local function readWhitespace()
            local c = text:sub(index, index)
            while true do
                readComment()
                if c == ' ' or c == '\t' or c == '\n' or c == '\r' then
                    index = index + 1
                else
                    break
                end
                if index > text:len() then
                    break
                end
                c = text:sub(index, index)
            end
        end


    public LuaFile Parse(string text)
    {
        file = new LuaFile();
        index = 1;
        lineNumber = 1;
        func = file.Main;
        file.Main.Vararg = 2;
        file.Main.Name = "LASM Chunk";
        funcStack = new Stack<Chunk>();
        
        readWhitespace()
        while text:sub(index, index) ~= "" do
            readWhitespace()
            local line = ""
            while true do
                local c = text:sub(index, index)
                if c == "\r" then
                    index = index + 1
                    if text:sub(index, index) == "\n" then
                        index = index + 1
                    end
                    break
                elseif c == "\n" then
                    index = index + 1
                    break
                elseif c == "" then
                    break
                else
                    line = line .. c
                end
                index = index + 1
            end
            while line:sub(1, 1) == " " or line:sub(1, 1) == "\t" do
                line = line:sub(2) -- strip whitespace
            end
            if line:sub(1, 1) == "." then
                parseControl(line)
            else if line == "" or line:sub(1, 1) == ";" then
        { }// do nothing.
            else
                local op = parseOpcode(line)
                if not op then error"Unable to parse opcode!" end
                op.LineNumber = lineNumber
                //table.insert(func.Instructions, op)
                // I CAN'T BELIEVE I HAD TO RESORT TO THIS CRAP TO GET IT TO ADD INSTRUCTIONS. I MEAN SERIOUSLY, LUA, COME ON...
                getmetatable(func.Instructions).__newindex(func.Instructions, func.Instructions.Count, op)
            end
            lineNumber = lineNumber + 1
        end
        
        local instr1 = func.Instructions[func.Instructions.Count - 1];
        local instr2 = Instruction:new("RETURN");
        instr2.A = 0;
        instr2.B = 1;
        instr2.C = 0;
        //getmetatable(func.Instructions).__newindex(func.Instructions, func.Instructions.Count, op)
        if instr1 then
            if instr1.Opcode ~= "RETURN" then
                getmetatable(func.Instructions).__newindex(func.Instructions, func.Instructions.Count, instr2)
                --table.insert(func.Instructions, func.Instructions.Count, instr2)
            end
        else
            getmetatable(func.Instructions).__newindex(func.Instructions, 0, instr2)
        end
        return file
    }
}
/*
if false then -- Testing. 
    local p = Parser:new()
    local file = p:Parse[[
    .const "print"
    .const "Hello"
    getglobal 0 0
    loadk 1 1
    call 0 2 1
    return 0 1
    ]]
    local code = file:Compile()
    local f = io.open("lasm.out", "wb")
    f:write(code)
    f:close()
    local funcX = { loadstring(code) }
    print(funcX[1], funcX[2])
    if funcX[1] then
        funcX[1]()
    end
    --table.foreach(file.Main.Instructions, function(x) pcall(function() print(x) end) end)
    --funcX()
end
*/
}
}