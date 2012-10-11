/*
** $Id: lua.h,v 1.218.1.5 2008/08/06 13:30:12 roberto Exp $
** Lua - An Extensible Extension Language
** Lua.org, PUC-Rio, Brazil (http://www.lua.org)
** See Copyright Notice at the end of this file
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpLua
{
    using lua_Number = Double;
    using lua_Integer = System.Int32;
    using lua_Unsigned = System.Int64;
    using TValue = Lua.lua_TValue;

    public partial class Lua
    {

        public const string LUA_VERSION = "Lua 5.1";
        public const string LUA_RELEASE = "Lua 5.1.5";
        public const int LUA_VERSION_NUM = 501;
        //public const string LUA_COPYRIGHT = "Copyright (C) 1994-2012 Lua.org, PUC-Rio";
        //public const string LUA_AUTHORS = "R. Ierusalimschy, L. H. de Figueiredo & W. Celes";
        public const string LUA_COPYRIGHT = "Copyright (C) 2012 LoDC";
        public const string LUA_AUTHORS = "Elijah Frederickson";


        /* mark for precompiled code (`<esc>Lua') */
        public const string LUA_SIGNATURE = "\x01bLua";

        /* option for multiple returns in `lua_pcall' and `lua_call' */
        public const int LUA_MULTRET = (-1);


        /*
        ** pseudo-indices
        */
        public const int LUA_REGISTRYINDEX = (-10000);
        public const int LUA_ENVIRONINDEX = (-10001);
        public const int LUA_GLOBALSINDEX = (-10002);
        public static int lua_upvalueindex(int i) { return LUA_GLOBALSINDEX - i; }


        /* thread status; 0 is OK */
        public const int LUA_YIELD = 1;
        public const int LUA_ERRRUN = 2;
        public const int LUA_ERRSYNTAX = 3;
        public const int LUA_ERRMEM = 4;
        public const int LUA_ERRERR = 5;


        public delegate int lua_CFunction(LuaState L);


        /*
        ** functions that read/write blocks when loading/dumping Lua chunks
        */
        public delegate CharPtr lua_Reader(LuaState L, object ud, out uint sz);

        public delegate int lua_Writer(LuaState L, CharPtr p, uint sz, object ud);


        /*
        ** prototype for memory-allocation functions
        */
        //public delegate object lua_Alloc(object ud, object ptr, uint osize, uint nsize);
        public delegate object lua_Alloc(Type t);


        /*
        ** basic types
        */
        public const int LUA_TNONE = -1;

        public const int LUA_TNIL = 0;
        public const int LUA_TBOOLEAN = 1;
        public const int LUA_TLIGHTUSERDATA = 2;
        public const int LUA_TNUMBER = 3;
        public const int LUA_TSTRING = 4;
        public const int LUA_TTABLE = 5;
        public const int LUA_TFUNCTION = 6;
        public const int LUA_TUSERDATA = 7;
        public const int LUA_TTHREAD = 8;



        /* minimum Lua stack available to a C function */
        public const int LUA_MINSTACK = 20;


        /* type of numbers in Lua */
        //typedef LUA_NUMBER lua_Number;


        /* type for integer functions */
        //typedef LUA_INTEGER lua_Integer;

        /*
        ** garbage-collection function and options
        */

        public const int LUA_GCSTOP = 0;
        public const int LUA_GCRESTART = 1;
        public const int LUA_GCCOLLECT = 2;
        public const int LUA_GCCOUNT = 3;
        public const int LUA_GCCOUNTB = 4;
        public const int LUA_GCSTEP = 5;
        public const int LUA_GCSETPAUSE = 6;
        public const int LUA_GCSETSTEPMUL = 7;

        /* 
        ** ===============================================================
        ** some useful macros
        ** ===============================================================
        */

        public static void lua_pop(LuaState L, int n)
        {
            lua_settop(L, -(n) - 1);
        }

        public static void lua_newtable(LuaState L)
        {
            lua_createtable(L, 0, 0);
        }

        public static void lua_register(LuaState L, CharPtr n, lua_CFunction f)
        {
            lua_pushcfunction(L, f);
            lua_setglobal(L, n);
        }

        public static void lua_pushcfunction(LuaState L, lua_CFunction f)
        {
            lua_pushcclosure(L, f, 0);
        }

        public static uint lua_strlen(LuaState L, int i)
        {
            return lua_objlen(L, i);
        }

        public static bool lua_isfunction(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TFUNCTION;
        }

        public static bool lua_istable(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TTABLE;
        }

        public static bool lua_islightuserdata(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TLIGHTUSERDATA;
        }

        public static bool lua_isnil(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TNIL;
        }

        public static bool lua_isboolean(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TBOOLEAN;
        }

        public static bool lua_isthread(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TTHREAD;
        }

        public static bool lua_isnone(LuaState L, int n)
        {
            return lua_type(L, n) == LUA_TNONE;
        }

        public static bool lua_isnoneornil(LuaState L, lua_Number n)
        {
            return lua_type(L, (int)n) <= 0;
        }

        public static void lua_pushliteral(LuaState L, CharPtr s)
        {
            //TODO: Implement use using lua_pushlstring instead of lua_pushstring
            //lua_pushlstring(L, "" s, (sizeof(s)/GetUnmanagedSize(typeof(char)))-1)
            lua_pushstring(L, s);
        }

        public static void lua_setglobal(LuaState L, CharPtr s)
        {
            lua_setfield(L, LUA_GLOBALSINDEX, s);
        }

        public static void lua_getglobal(LuaState L, CharPtr s)
        {
            lua_getfield(L, LUA_GLOBALSINDEX, s);
        }

        public static CharPtr lua_tostring(LuaState L, int i)
        {
            uint blah;
            return lua_tolstring(L, i, out blah);
        }

        ////#define lua_open()	luaL_newstate()
        public static LuaState lua_open()
        {
            return luaL_newstate();
        }

        ////#define lua_getregistry(L)	lua_pushvalue(L, LUA_REGISTRYINDEX)
        public static void lua_getregistry(LuaState L)
        {
            lua_pushvalue(L, LUA_REGISTRYINDEX);
        }

        ////#define lua_getgccount(L)	lua_gc(L, LUA_GCCOUNT, 0)
        public static int lua_getgccount(LuaState L)
        {
            return lua_gc(L, LUA_GCCOUNT, 0);
        }

        public static lua_Unsigned luaL_checkunsigned(LuaState L, int narg)
        {
            bool isnum = false;
            lua_Unsigned d = lua_tounsignedx(L, narg, ref isnum);
            if (!isnum)
                tag_error(L, narg, LUA_TNUMBER);
            return d;
        }

        public static lua_Unsigned lua_tounsignedx(LuaState L, int idx, ref bool isnum)
        {
            TValue n = null;
            TValue o = index2adr(L, idx);
            if (tonumber(ref o, n) == 1)
            {
                lua_Unsigned res;
                lua_Number num = nvalue(o);
                res = (lua_Unsigned)num;
                if (isnum)
                    isnum = true;
                return res;
            }
            else
            {
                if (isnum)
                    isnum = false;
                return 0;
            }
        }

        public static void lua_pushunsigned(LuaState L, lua_Unsigned u)
        {
            lua_Number n;
            lua_lock(L);
            n = (((u) <= (lua_Unsigned)int.MaxValue) ? (lua_Number)(int)(u) : (lua_Number)(u));
            setnvalue(L.top, n);
            api_incr_top(L);
            lua_unlock(L);
        }




        //#define lua_Chunkreader		lua_Reader
        //#define lua_Chunkwriter		lua_Writer


        /*
        ** {======================================================================
        ** Debug API
        ** =======================================================================
        */


        /*
        ** Event codes
        */
        public const int LUA_HOOKCALL = 0;
        public const int LUA_HOOKRET = 1;
        public const int LUA_HOOKLINE = 2;
        public const int LUA_HOOKCOUNT = 3;
        public const int LUA_HOOKTAILRET = 4;


        /*
        ** Event masks
        */
        public const int LUA_MASKCALL = (1 << LUA_HOOKCALL);
        public const int LUA_MASKRET = (1 << LUA_HOOKRET);
        public const int LUA_MASKLINE = (1 << LUA_HOOKLINE);
        public const int LUA_MASKCOUNT = (1 << LUA_HOOKCOUNT);

        /* Functions to be called by the debuger in specific events */
        public delegate void lua_Hook(LuaState L, lua_Debug ar);


        public class lua_Debug
        {
            public int event_;
            public CharPtr name;	/* (n) */
            public CharPtr namewhat;	/* (n) `global', `local', `field', `method' */
            public CharPtr what;	/* (S) `Lua', 'C' => 'CLR', `main', `tail' */
            public CharPtr source;	/* (S) */
            public int currentline;	/* (l) */
            public int nups;		/* (u) number of upvalues */
            public int linedefined;	/* (S) */
            public int lastlinedefined;	/* (S) */
            public CharPtr short_src = new char[LUA_IDSIZE]; /* (S) */
            /* private part */
            public int i_ci;  /* active function */
        };

        /* }====================================================================== */


        /******************************************************************************
        * Copyright (C) 1994-2012 Lua.org, PUC-Rio.  All rights reserved.
        *
        * Permission is hereby granted, free of charge, to any person obtaining
        * a copy of this software and associated documentation files (the
        * "Software"), to deal in the Software without restriction, including
        * without limitation the rights to use, copy, modify, merge, publish,
        * distribute, sublicense, and/or sell copies of the Software, and to
        * permit persons to whom the Software is furnished to do so, subject to
        * the following conditions:
        *
        * The above copyright notice and this permission notice shall be
        * included in all copies or substantial portions of the Software.
        *
        * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
        * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
        * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
        * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
        * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        ******************************************************************************/

    }
}
