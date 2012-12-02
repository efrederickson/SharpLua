using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;

namespace lbci
{
    using Proto = Lua.Proto;
    using LuaState = Lua.LuaState;
    using Instruction = System.UInt32;
    using OpCode = Lua.OpCode;

    public class LBCI
    {
        static Proto getproto(LuaState L, int i = 1)
        {
            if (Lua.lua_isuserdata(L, i) == 1)
                return (Proto)Lua.lua_touserdata(L, i);
            if (!Lua.lua_isfunction(L, i) || Lua.lua_iscfunction(L, i))
                Lua.luaL_typerror(L, i, "Lua function");
            return ((Lua.Closure)Lua.lua_topointer(L, i)).l.p;
        }

        public static int getupvalue(LuaState L)
        {
            Proto f = getproto(L, 1);
            int i = Lua.luaL_checkinteger(L, 2);
            if (i <= 0 || i > f.sizeupvalues || f.upvalues == null)
                return 0;
            i--;
            Lua.lua_pushstring(L, f.upvalues[i].str);
            return 1;
        }

        public static int getlocal(LuaState L)
        {
            Proto f = getproto(L, 1);
            int i = Lua.luaL_checkinteger(L, 2);
            if (i <= 0 || i > f.sizelocvars || f.locvars == null)
                return 0;
            i--;
            Lua.lua_pushstring(L, f.locvars[i].varname.str);
            Lua.lua_pushinteger(L, f.locvars[i].startpc + 1);
            Lua.lua_pushinteger(L, f.locvars[i].endpc + 1);
            return 3;
        }

        public static int getconstant(LuaState L)
        {
            Proto f = getproto(L, 1);
            int i = Lua.luaL_checkinteger(L, 2);
            if (i <= 0 || i > f.sizek || f.k == null)
                return 0;
            i--;
            Lua.lua_pushnil(L);
            L.top[-1] = f.k[i];
            return 1;
        }

        public static int getfunction(LuaState L)
        {
            Proto f = getproto(L, 1);
            int i = Lua.luaL_checkinteger(L, 2);
            if (i <= 0 || i > f.sizep)
                return 0;
            i--;
            Lua.lua_pushlightuserdata(L, f.p[i]);
            return 1;
        }

        public static int getinstruction(LuaState L)
        {
            Proto f = getproto(L, 1);
            int pc = Lua.luaL_checkinteger(L, 2);
            if (pc <= 0 || pc > f.sizecode || f.code == null)
                return 0;
            pc--;
            {
                Instruction[] code = f.code;
                Instruction i = code[pc];
                OpCode o = Lua.GET_OPCODE(i);
                int a = Lua.GETARG_A(i);
                int b = Lua.GETARG_B(i);
                int c = Lua.GETARG_C(i);
                int bx = Lua.GETARG_Bx(i);
                int sbx = Lua.GETARG_sBx(i);
                int line = Lua.getline(f, pc);
                if (line > 0)
                    Lua.lua_pushinteger(L, line);
                else
                    Lua.lua_pushnil(L);
                Lua.lua_pushstring(L, Lua.luaP_opnames[(int)o]);
                switch (Lua.getOpMode(o))
                {
                    case Lua.OpMode.iABC:
                        Lua.lua_pushinteger(L, a);
                        if (Lua.getBMode(o) != Lua.OpArgMask.OpArgN)
                            Lua.lua_pushinteger(L, Lua.ISK(b) == 1 ? (-1 - Lua.INDEXK(b)) : b);
                        else
                            Lua.lua_pushnil(L);
                        if (Lua.getCMode(o) != Lua.OpArgMask.OpArgN)
                            Lua.lua_pushinteger(L, Lua.ISK(c) == 1 ? (-1 - Lua.INDEXK(c)) : c);
                        else
                            Lua.lua_pushnil(L);
                        break;
                    case Lua.OpMode.iABx:
                        Lua.lua_pushinteger(L, a);
                        if (Lua.getBMode(o) == Lua.OpArgMask.OpArgK)
                            Lua.lua_pushinteger(L, -1 - bx);
                        else
                            Lua.lua_pushinteger(L, bx);
                        Lua.lua_pushnil(L);
                        break;
                    case Lua.OpMode.iAsBx:
                        if (o != OpCode.OP_JMP)
                            Lua.lua_pushinteger(L, a);
                        Lua.lua_pushinteger(L, sbx);
                        Lua.lua_pushnil(L);
                        break;
                }
                switch (o)
                {
                    case OpCode.OP_JMP:
                    case OpCode.OP_FORLOOP:
                    case OpCode.OP_FORPREP:
                        Lua.lua_pop(L, 1);
                        Lua.lua_pushinteger(L, sbx + pc + 2);
                        Lua.lua_pushnil(L);
                        break;
                    default:
                        break;
                }
            }
            return 5;
        }

        static void setsfield(LuaState L, Lua.CharPtr n, Lua.CharPtr v)
        {
            Lua.lua_pushstring(L, v);
            Lua.lua_setfield(L, -2, n);
        }

        static void setifield(LuaState L, Lua.CharPtr n, int v)
        {
            Lua.lua_pushinteger(L, v);
            Lua.lua_setfield(L, -2, n);
        }

        static void setbfield(LuaState L, Lua.CharPtr n, int v)
        {
            Lua.lua_pushboolean(L, v);
            Lua.lua_setfield(L, -2, n);
        }


        static int getheader(LuaState L)
        {
            Proto f = getproto(L, 1);
            Lua.CharPtr s = f.source.str;
            if (s[0] == '@' || s[0] == '=')
                s.index++;
            else if (s[0] == Lua.LUA_SIGNATURE[0])
                s = "(binary string)";
            else
                s = "(string)";
            Lua.lua_newtable(L);
            setsfield(L, "source", s);
            setifield(L, "line", f.linedefined);
            setifield(L, "lastline", f.lastlinedefined);
            setifield(L, "instructions", f.sizecode);
            setifield(L, "params", f.numparams);
            setbfield(L, "isvararg", f.is_vararg);
            setifield(L, "slots", f.maxstacksize);
            setifield(L, "upvalues", f.nups);
            setifield(L, "locals", f.sizelocvars);
            setifield(L, "constants", f.sizek);
            setifield(L, "functions", f.sizep);
            return 1;
        }

        static int setconstant(LuaState L)
        {
            Proto f = getproto(L, 1);
            int i = Lua.luaL_checkinteger(L, 2);
            if (i <= 0 || i > f.sizek || f.k == null)
                return 0;
            i--;
            Lua.lua_settop(L, 3);
            f.k[i] = L.top[-1];
            return 0;
        }

        static readonly Lua.luaL_Reg[] funcs = {
            new Lua.luaL_Reg("getupvalue", getupvalue),
            new Lua.luaL_Reg("getconstant", getconstant),
            new Lua.luaL_Reg("getfunction", getfunction),
            new Lua.luaL_Reg("getheader", getheader),
            new Lua.luaL_Reg("getinstruction", getinstruction),
            new Lua.luaL_Reg("getlocal", getlocal),
            new Lua.luaL_Reg("setconstant", setconstant),
            new Lua.luaL_Reg(null, null),
        };

        public static int luaopen_lbci(LuaState L)
        {
            Lua.luaL_register(L, "inspector", funcs);
            return 1;
        }
    }
}
