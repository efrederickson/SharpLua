-- XFU5K470R - Advanced Lua Obfuscator
-- Copyright (C) 2012 LoDC
-- I lost the actual source code when my file server hard drive corrupted. 
-- This is the best backup i could find

function obfuscate(code, level, mxLevel)
    local function print(...) end
    local concat = function(...) return table.concat({...}, "") end
    math.randomseed(os and os.time() or tick())
    level = level or 1
    mxLevel = mxLevel or 3
    
    local a = ""
    -- STRIP COMMENTS
    code = code:gsub("(%-%-%[(=*)%[.-%]%2%])", "")
    code = code:gsub("(%-%-[^\r\n]*)", "")
    
    -- RIP ALL CONSTANT STRINGS OUT
    local function dumpString(x) return concat("\"", x:gsub(".", function(d) return "\\" .. string.byte(d) end), "\"") end
    local function dumpString2(x) 
        local x2 = "\""
        local x3 = ""
        for _,__ in x:gmatch("%[(=*)%[(.-)%]%1%]") do
            x3 = __:gsub(".", function(d) return "\\" .. string.byte(d) end)
        end
        return concat(x2, x3, "\"")
    end
    local function GenerateSomeFluff()
        local randomTable = { "N00BING N00B TABLE", "game.Workspace:ClearAllChildren()", "?????????", "game", "Workspace", "wait", "loadstring", "Lighting", "TeleportService", "error", "crash__", "_", "____", "\\\"FOOLED YA?!?!\\\"", "\\\"MWAHAHA H4X0RZ\\\"", "string", "table", "\\\"KR3D17 70 XFU5K470R\\\"", "string", "os", "tick", "\"system\"" }
        --for i = 1, 100 do print(math.random(1, #randomTable)) end
        local x = math.random(1, #randomTable)
        if x > (#randomTable / 2) then
            local randomName = randomTable[x]
            return concat("local ", string.rep("_", math.random(5, 10)), " = ", "____[#____ - 9](", dumpString("loadstring(\"return " .. randomName .. "\")()"), ")\n")
        elseif x > 3 then
            return concat("local ", string.rep("_", math.random(5, 10)), " = ____[", math.random(1, 31), "]\n")
        else -- x == 3, 2, or 1
            return concat("local ", ("_"):rep(100), " = ", dumpString("XFU5K470R R00LZ"), "\n")
        end
    end
    local function GenerateFluff() 
        --local x = { } for i = 1, math.random(2, 10) do table.insert(x, GenerateSomeFluff()) end return table.concat(x) 
        return GenerateSomeFluff()
    end
    
    --a = "local _ = function(x) return string.char(x) end "
    a = a .. "local CONSTANT_POOL = { "
    local CONSTANT_POOL = { }
    local i = 0
    -- "
    local last = ""
    local instr = false
    local foundOne = true
    while foundOne do
        foundOne = false
        for i2 = 1, code:len() do
            local c = code:sub(i2, i2)
            if c == "\"" then
                if code:sub(i2 - 1, i2 - 1) == "\\" then
                    if instr then
                        last = last .. "\""
                    end
                else
                    instr = not instr
                    if not instr then
                        if not CONSTANT_POOL[last] then
                            CONSTANT_POOL[last] = i
                            a = a .. "[" .. i .. "]" .. " = " .. dumpString(last) .. ", "
                            code = code:gsub("\"" .. last .. "\"", "(CONSTANT_POOL[" .. CONSTANT_POOL[last] .. "])")
                            i = i + 1
                        else
                            code = code:gsub("\"" .. last .. "\"", "(CONSTANT_POOL[" .. CONSTANT_POOL[last] .. "])")
                        end
                        last = ""
                        foundOne = true
                        break
                    end
                end
            else
                if instr then
                    last = last .. c
                end
            end
        end
    end
    -- '
    local last = ""
    local instr = false
    local foundOne = true
    while foundOne do
        foundOne = false
        for i2 = 1, code:len() do
            local c = code:sub(i2, i2)
            if c == "'" then
                if code:sub(i2 - 1, i2 - 1) == "\\" then
                    if instr then
                        last = last .. "'"
                    end
                else
                    instr = not instr
                    if not instr then
                        if not CONSTANT_POOL[last] then
                            CONSTANT_POOL[last] = i
                            a = a .. "[" .. i .. "]" .. " = " .. dumpString(last) .. ", "
                            code = code:gsub("'" .. last .. "'", "(CONSTANT_POOL[" .. CONSTANT_POOL[last] .. "])")
                            i = i + 1
                        else
                            code = code:gsub("'" .. last .. "'", "(CONSTANT_POOL[" .. CONSTANT_POOL[last] .. "])")
                        end
                        last = ""
                        foundOne = true
                        break
                    end
                end
            else
                if instr then
                    last = last .. c
                end
            end
        end
    end
    -- [(=*)[
    for var in code:gmatch("(%[(=*)%[.*%]%2%])") do
    --for var in code:gmatch("%[(=*)%[.-%]%1%]") do 
        if not CONSTANT_POOL[var] then
            a = a .. "[" .. i .. "]" .. " = " .. dumpString2(var) .. ", "
            CONSTANT_POOL[var] = i
            for i2 = 1, code:len() do
                if code:sub(i2, i2 + var:len() - 1) == var then
                    code = code:sub(1, i2 - 1) .. "(CONSTANT_POOL[" .. i .. "])" .. code:sub(i2 + var:len())
                end
            end
            --code = code:gsub(var, "(CONSTANT_POOL[" .. CONSTANT_POOL[var] .. "])")
            i = i + 1
        end
    end
    a = a .. concat("[", i, "] = \"\88\70\85\53\75\52\55\48\82\32\49\53\32\52\87\51\53\48\77\51\46\32\75\82\51\68\49\55\32\55\48\32\88\70\85\53\75\52\55\48\82\33\"")
    a = a .. " }\n"
--print(code)

    -- SOME CHANGERS
    
    local _VAR_NAME_ = 0
    local function genVarName()
        _VAR_NAME_ = _VAR_NAME_ + 1
        return "_" .. _VAR_NAME_
    end

if level == 1 then
    local function GetReplacement()
        local s = ""
        for i = 1, math.random(6,20) do
            s = s .. string.char(math.random(65,90)) --97,123 lowercase
        end
        return s
    end
    local library = {}
	-- functions
	for fType in code:gmatch("function ([%w_]+)%(") do -- for fType in x:gmatch("(function %a+%(%))") do
		local replacement = GetReplacement()
		--print(fType)
		if #fType > 5 then
			library[fType] = replacement
			code = code:gsub("function " .. fType, "function " .. replacement)
		end
	end
	for fCall in code:gmatch("([%w_]+)%(") do -- for fCall in x:gmatch("(%a+)%(%)") do
		if library[fCall] then
			code = code:gsub(fCall .. '%(', library[fCall] .. '%(')
		end
	end
	-- variables
	--[[for lType in code:gmatch("(local [%w_]+)") do
		local replacement = GetReplacement()
		local varName = lType:sub(7)
		if #varName > 4 then
			library[varName] = replacement
			code = code:gsub(lType, "local " .. replacement)
		end
	end]]
    local function isKeyword(s)
        local s = "and break do else elseif end false for function if in local nil not or repeat return then true until"
        for w in s:gmatch("(%w+)") do
            --if w:find(s) ~= nil then
            --    return true
            --end
            for i = 1, w:len() do
                if s:sub(1, i) == w:sub(1, i) then
                    return true
                end
            end
        end
        return false
    end
    
	for each in code:gmatch("([%w_]*)%s*=") do
        if #each > 3 and not isKeyword(each) then
        --each:sub(1, 4) ~= "loca" and
        --each:sub(1, 5) ~= "local" and
        --each:sub(1, 5) ~= "funct" and 
        --each:sub(1, 6) ~= "functi" and 
        --each:sub(1, 7) ~= "functio" and 
        --each:sub(1, 8) ~= "function" then
            local varName = GetReplacement()
            code = code:gsub(each, varName)
            --code = code:gsub("%[" .. name, "[" .. newName)
            --code = code:gsub("%(" .. name, "(" .. newName)
            --code = code:gsub(name .. "(%s)%.", newName .. "%1.")
            --code = code:gsub(name .. "%s*=", newName .. " =")
            --code = code:gsub(name .. "%s*([+-/*%^])", newName .. " " .. "%2")
        end
	end
end


    -- STRIP ALL WHITESPACE (AT THIS POINT IT DOESN'T MATTER FOR VARIABLES, THEY'RE GONE)
    code = code:gsub("[\n]+", " ")
    code = code:gsub("[\r]+", " ")
    code = code:gsub("[\t]+", " ")
    code = code:gsub("[ ]+", " ") -- '  ' becomes ' '
    a = a .. code
    
    -- CONVERT TO CHAR/TABLE/LOADSTRING THING
    math.randomseed(os and os.time() or tick())
    local __X = math.random()
    local a2 = [[ math.randomseed(]] .. __X .. [[)
____ = { function(...) local t = { ...} return ____[8](t) end, print, game, math.frexp, math.random(1, 1100), string.dump, string.sub, table.concat, wait, tick, loadstring, "t", function(x) local x2 = loadstring(x) if x2 then return ____[tonumber("\50\48")](function() x2() end) else return nil end end, "InsertService", 1234567890, getfenv, "", "wai", 7.2, pcall, math.pi, ""}
]] .. GenerateFluff() .. [[local ___ = ____[5]
]] .. GenerateFluff() .. [[local _ = function(x) return string.char(x / ___) end
]] .. GenerateFluff() .. [[local __ = {]]
    math.randomseed(__X)
    local ___X = math.random(1, 1100)
    local a3 = { }
    for i = 1, a:len() do
        table.insert(a3, concat("_(", (string.byte(a:sub(i, i)) * ___X), "), "))
    end
    a2 = a2 .. table.concat(a3, "")
    a2 = a2 .. " } \n"
    a2 = a2 .. GenerateFluff()
    a2 = a2 .. "return ____[11]((____[8](__)), ____[#____])()\n"
    --a2 = a2 .. GenerateFluff() TODO
    if level < mxLevel then -- more than 3 is over 1MB (for the simple test). WAY to long to use in a project.
        print(concat("OBFUSCATED AT LEVEL ", level, " OUT OF ", mxLevel, " (" .. a:len() .. " Obfuscated characters)"))
        return obfuscate(a2, level + 1, mxLevel)
    else
        print(concat("OBFUSCATED AT LEVEL ", level, " OUT OF ", mxLevel, " (", a:len(), " Obfuscated Characters) [Done]"))
        a2 = a2:gsub("[\n]+", " ")
        a2 = a2:gsub("[\r]+", " ")
        a2 = a2:gsub("[\t]+", " ")
        a2 = a2:gsub("[ ]+", " ") -- '  ' becomes ' '
        return a2
    end
end

xfuscate = function(code)
    return obfuscate(code, 1, 1)
end

code = [[
print(CONSTANT_POOL[0])
]]

if arg and arg[1] then
    code = arg[1]
end

result = obfuscate(code, 1, 3)
print(result)
a, b = loadstring(result)
if a then
    print"--Successful!"
else
    print("--Failed: " .. b)
end