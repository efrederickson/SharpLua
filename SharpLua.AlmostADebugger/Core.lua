clr.load"SharpLua.LASM"
lasm = luanet.namespace"SharpLua.LASM"
dis = lasm.Disassembler
assert(dis, "Could not load LASM Disassembler!")
--print(lasm, lasm.Disassembler)
func = nil
function Load(s)
    local a = loadstring(s)
    func = a
    if a then
        return a, dis.Disassemble(string.dump(a))
    else 
        return a
    end
end