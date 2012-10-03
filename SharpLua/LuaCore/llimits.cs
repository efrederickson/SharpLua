//#define lua_assert

/*
** $Id: llimits.h,v 1.69.1.1 2007/12/27 13:02:25 roberto Exp $
** Limits, basic types, and some other `installation-dependent' definitions
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SharpLua
{
    using lu_int32 = System.UInt32;
    using lu_mem = System.UInt32;
    using l_mem = System.Int32;
    using lu_byte = System.Byte;
    using l_uacNumber = System.Double;
    using lua_Number = System.Double;
    using Instruction = System.UInt32;

    public partial class Lua
    {

        //typedef LUAI_UINT32 lu_int32;

        //typedef LUAI_UMEM lu_mem;

        //typedef LUAI_MEM l_mem;



        /* chars used as small naturals (so that `char' is reserved for characters) */
        //typedef unsigned char lu_byte;


        public const uint MAX_SIZET = uint.MaxValue - 2;

        public const lu_mem MAX_LUMEM = lu_mem.MaxValue - 2;


        public const int MAX_INT = (Int32.MaxValue - 2);  /* maximum value of an int (-2 for safety) */

        /*
        ** conversion of pointer to integer
        ** this is for hashing only; there is no problem if the integer
        ** cannot hold the whole pointer value
        */
        //#define IntPoint(p)  ((uint)(lu_mem)(p))



        /* type to ensure maximum alignment */
        //typedef LUAI_USER_ALIGNMENT_T L_Umaxalign;


        /* result of a `usual argument conversion' over lua_Number */
        //typedef LUAI_UACNUMBER l_uacNumber;


        /* internal assertions for in-house debugging */

#if lua_assert

		[Conditional("DEBUG")]
		public static void lua_assert(bool c) {Debug.Assert(c);}

		[Conditional("DEBUG")]
		public static void lua_assert(int c) { Debug.Assert(c != 0); }

		public static object check_exp(bool c, object e)		{lua_assert(c); return e;}
		public static object check_exp(int c, object e) { lua_assert(c != 0); return e; }

#else

        [Conditional("DEBUG")]
        public static void lua_assert(bool c) { }

        [Conditional("DEBUG")]
        public static void lua_assert(int c) { }

        public static object check_exp(bool c, object e) { return e; }
        public static object check_exp(int c, object e) { return e; }

#endif

        [Conditional("DEBUG")]
        public static void api_check(object o, bool e) { lua_assert(e); }
        public static void api_check(object o, int e) { lua_assert(e != 0); }

        //#define UNUSED(x)	((void)(x))	/* to avoid warnings */


        public static lu_byte cast_byte(int i) { return (lu_byte)i; }
        public static lu_byte cast_byte(long i) { return (lu_byte)(int)i; }
        public static lu_byte cast_byte(bool i) { return i ? (lu_byte)1 : (lu_byte)0; }
        public static lu_byte cast_byte(lua_Number i) { return (lu_byte)i; }
        public static lu_byte cast_byte(object i) { return (lu_byte)(int)(i); }

        public static int cast_int(int i) { return (int)i; }
        public static int cast_int(uint i) { return (int)i; }
        public static int cast_int(long i) { return (int)(int)i; }
        public static int cast_int(ulong i) { return (int)(int)i; }
        public static int cast_int(bool i) { return i ? (int)1 : (int)0; }
        public static int cast_int(lua_Number i) { return (int)i; }
        public static int cast_int(object i) { Debug.Assert(false, "Can't convert int."); return Convert.ToInt32(i); }

        public static lua_Number cast_num(int i) { return (lua_Number)i; }
        public static lua_Number cast_num(uint i) { return (lua_Number)i; }
        public static lua_Number cast_num(long i) { return (lua_Number)i; }
        public static lua_Number cast_num(ulong i) { return (lua_Number)i; }
        public static lua_Number cast_num(bool i) { return i ? (lua_Number)1 : (lua_Number)0; }
        public static lua_Number cast_num(object i) { Debug.Assert(false, "Can't convert number."); return Convert.ToSingle(i); }

        /*
        ** type for virtual-machine instructions
        ** must be an unsigned with (at least) 4 bytes (see details in lopcodes.h)
        */
        //typedef lu_int32 Instruction;



        /* maximum stack for a Lua function */
        public const int MAXSTACK = 250;



        /* minimum size for the string table (must be power of 2) */
        public const int MINSTRTABSIZE = 32;


        /* minimum size for string buffer */
        public const int LUA_MINBUFFER = 32;


#if !lua_lock
        public static void lua_lock(LuaState L) { }
        public static void lua_unlock(LuaState L) { }
#endif


#if !luai_threadyield
        public static void luai_threadyield(LuaState L) { lua_unlock(L); lua_lock(L); }
#endif


        /*
		** macro to control inclusion of some hard tests on stack reallocation
		*/
        //#ifndef HARDSTACKTESTS
        //#define condhardstacktests(x)	((void)0)
        //#else
        //#define condhardstacktests(x)	x
        //#endif

    }
}
