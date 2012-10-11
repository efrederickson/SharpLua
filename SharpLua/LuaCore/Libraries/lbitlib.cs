/*
** $Id: lbitlib.c,v 1.16 2011/06/20 16:35:23 roberto Exp $
** Standard library for bitwise operations
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Note: Defined as two libraries, bit and bit32

namespace SharpLua
{
    using lua_Unsigned = System.Int64;
    using b_uint = System.Int64;

    public partial class Lua
    {

        /* number of bits to consider in a number */
        public const int LUA_NBITS = 32;


        public const lua_Unsigned ALLONES = (~(((~(lua_Unsigned)0) << (LUA_NBITS - 1)) << 1));

        /* macro to trim extra bits */
        public static lua_Unsigned trim(lua_Unsigned x)
        {
            return ((x) & ALLONES);
        }


        /* builds a number with 'n' ones (1 <= n <= LUA_NBITS) */
        public static lua_Unsigned mask(int n)
        {
            return (~((ALLONES << 1) << ((n) - 1)));
        }

        static b_uint andaux(LuaState L)
        {
            int i, n = lua_gettop(L);
            b_uint r = ~(b_uint)0;
            for (i = 1; i <= n; i++)
                r &= luaL_checkunsigned(L, i);
            return trim(r);
        }


        static int b_and(LuaState L)
        {
            b_uint r = andaux(L);
            lua_pushunsigned(L, r);
            return 1;
        }


        static int b_test(LuaState L)
        {
            b_uint r = andaux(L);
            lua_pushboolean(L, r == 0 ? 0 : 1);
            return 1;
        }


        static int b_or(LuaState L)
        {
            int i, n = lua_gettop(L);
            b_uint r = 0;
            for (i = 1; i <= n; i++)
                r |= luaL_checkunsigned(L, i);
            lua_pushunsigned(L, trim(r));
            return 1;
        }


        static int b_xor(LuaState L)
        {
            int i, n = lua_gettop(L);
            b_uint r = 0;
            for (i = 1; i <= n; i++)
                r ^= luaL_checkunsigned(L, i);
            lua_pushunsigned(L, trim(r));
            return 1;
        }


        static int b_not(LuaState L)
        {
            b_uint r = ~luaL_checkunsigned(L, 1);
            lua_pushunsigned(L, trim(r));
            return 1;
        }


        static int b_shift(LuaState L, b_uint r, int i)
        {
            if (i < 0)
            {  /* shift right? */
                i = -i;
                r = trim(r);
                if (i >= LUA_NBITS) r = 0;
                else r >>= i;
            }
            else
            {  /* shift left */
                if (i >= LUA_NBITS) r = 0;
                else r <<= i;
                r = trim(r);
            }
            lua_pushunsigned(L, r);
            return 1;
        }


        static int b_lshift(LuaState L)
        {
            return b_shift(L, luaL_checkunsigned(L, 1), luaL_checkint(L, 2));
        }


        static int b_rshift(LuaState L)
        {
            return b_shift(L, luaL_checkunsigned(L, 1), -luaL_checkint(L, 2));
        }


        static int b_arshift(LuaState L)
        {
            b_uint r = luaL_checkunsigned(L, 1);
            int i = luaL_checkint(L, 2);
            if (i < 0 || (r & ((b_uint)1 << (LUA_NBITS - 1))) == 0)
                return b_shift(L, r, -i);
            else
            {  /* arithmetic shift for 'negative' number */
                if (i >= LUA_NBITS) r = ALLONES;
                else
                    r = trim((r >> i) | ~(~(b_uint)0 >> i));  /* add signal bit */
                lua_pushunsigned(L, r);
                return 1;
            }
        }


        static int b_rot(LuaState L, int i)
        {
            b_uint r = luaL_checkunsigned(L, 1);
            i &= (LUA_NBITS - 1);  /* i = i % NBITS */
            r = trim(r);
            r = (r << i) | (r >> (LUA_NBITS - i));
            lua_pushunsigned(L, trim(r));
            return 1;
        }


        static int b_lrot(LuaState L)
        {
            return b_rot(L, luaL_checkint(L, 2));
        }


        static int b_rrot(LuaState L)
        {
            return b_rot(L, -luaL_checkint(L, 2));
        }


        /*
        ** get field and width arguments for field-manipulation functions,
        ** checking whether they are valid
        */
        static int fieldargs(LuaState L, int farg, ref int width)
        {
            int f = luaL_checkint(L, farg);
            int w = luaL_optint(L, farg + 1, 1);
            luaL_argcheck(L, 0 <= f, farg, "field cannot be negative");
            luaL_argcheck(L, 0 < w, farg + 1, "width must be positive");
            if (f + w > LUA_NBITS)
                luaL_error(L, "trying to access non-existent bits");
            width = w;
            return f;
        }


        static int b_extract(LuaState L)
        {
            int w = 0;
            b_uint r = luaL_checkunsigned(L, 1);
            int f = fieldargs(L, 2, ref w);
            r = (r >> f) & mask(w);
            lua_pushunsigned(L, r);
            return 1;
        }


        static int b_replace(LuaState L)
        {
            int w = 0;
            b_uint r = luaL_checkunsigned(L, 1);
            b_uint v = luaL_checkunsigned(L, 2);
            int f = fieldargs(L, 3, ref w);
            lua_Unsigned m = mask(w);
            v &= m;  /* erase bits outside given width */
            r = (r & ~(m << f)) | (v << f);
            lua_pushunsigned(L, r);
            return 1;
        }

        static luaL_Reg[] bitlib = {
  new luaL_Reg("arshift", b_arshift),
  new luaL_Reg("band", b_and),
  new luaL_Reg("bnot", b_not),
  new luaL_Reg("bor", b_or),
  new luaL_Reg("bxor", b_xor),
  new luaL_Reg("btest", b_test),
  new luaL_Reg("extract", b_extract),
  new luaL_Reg("lrotate", b_lrot),
  new luaL_Reg("lshift", b_lshift),
  new luaL_Reg("replace", b_replace),
  new luaL_Reg("rrotate", b_rrot),
  new luaL_Reg("rshift", b_rshift),
  new luaL_Reg(null, null)
};



        public static int luaopen_bit32(LuaState L)
        {
            luaL_register(L, LUA_BITLIBNAME, bitlib);
            luaL_register(L, LUA_BITLIB32NAME, bitlib);
            return 1;
        }


    }
}
