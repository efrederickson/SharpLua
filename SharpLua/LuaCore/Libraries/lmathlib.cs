/*
** $Id: lmathlib.c,v 1.67.1.1 2007/12/27 13:02:25 roberto Exp $
** Standard mathematical library
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    using lua_Number = System.Double;

    public partial class Lua
    {
        public const double PI = 3.14159265358979323846;
        public const double RADIANS_PER_DEGREE = PI / 180.0;



        private static int math_abs(LuaState L)
        {
            lua_pushnumber(L, Math.Abs(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_sin(LuaState L)
        {
            lua_pushnumber(L, Math.Sin(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_sinh(LuaState L)
        {
            lua_pushnumber(L, Math.Sinh(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_cos(LuaState L)
        {
            lua_pushnumber(L, Math.Cos(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_cosh(LuaState L)
        {
            lua_pushnumber(L, Math.Cosh(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_tan(LuaState L)
        {
            lua_pushnumber(L, Math.Tan(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_tanh(LuaState L)
        {
            lua_pushnumber(L, Math.Tanh(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_asin(LuaState L)
        {
            lua_pushnumber(L, Math.Asin(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_acos(LuaState L)
        {
            lua_pushnumber(L, Math.Acos(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_atan(LuaState L)
        {
            lua_pushnumber(L, Math.Atan(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_atan2(LuaState L)
        {
            lua_pushnumber(L, Math.Atan2(luaL_checknumber(L, 1), luaL_checknumber(L, 2)));
            return 1;
        }

        private static int math_ceil(LuaState L)
        {
            lua_pushnumber(L, Math.Ceiling(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_floor(LuaState L)
        {
            lua_pushnumber(L, Math.Floor(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_fmod(LuaState L)
        {
            lua_pushnumber(L, fmod(luaL_checknumber(L, 1), luaL_checknumber(L, 2)));
            return 1;
        }

        private static int math_modf(LuaState L)
        {
            double ip;
            double fp = modf(luaL_checknumber(L, 1), out ip);
            lua_pushnumber(L, ip);
            lua_pushnumber(L, fp);
            return 2;
        }

        private static int math_sqrt(LuaState L)
        {
            lua_pushnumber(L, Math.Sqrt(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_pow(LuaState L)
        {
            lua_pushnumber(L, Math.Pow(luaL_checknumber(L, 1), luaL_checknumber(L, 2)));
            return 1;
        }

        private static int math_log(LuaState L)
        {
            lua_pushnumber(L, Math.Log(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_log10(LuaState L)
        {
            lua_pushnumber(L, Math.Log10(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_exp(LuaState L)
        {
            lua_pushnumber(L, Math.Exp(luaL_checknumber(L, 1)));
            return 1;
        }

        private static int math_deg(LuaState L)
        {
            lua_pushnumber(L, luaL_checknumber(L, 1) / RADIANS_PER_DEGREE);
            return 1;
        }

        private static int math_rad(LuaState L)
        {
            lua_pushnumber(L, luaL_checknumber(L, 1) * RADIANS_PER_DEGREE);
            return 1;
        }

        private static int math_frexp(LuaState L)
        {
            int e;
            lua_pushnumber(L, frexp(luaL_checknumber(L, 1), out e));
            lua_pushinteger(L, e);
            return 2;
        }

        private static int math_ldexp(LuaState L)
        {
            lua_pushnumber(L, ldexp(luaL_checknumber(L, 1), luaL_checkint(L, 2)));
            return 1;
        }



        private static int math_min(LuaState L)
        {
            int n = lua_gettop(L);  /* number of arguments */
            lua_Number dmin = luaL_checknumber(L, 1);
            int i;
            for (i = 2; i <= n; i++)
            {
                lua_Number d = luaL_checknumber(L, i);
                if (d < dmin)
                    dmin = d;
            }
            lua_pushnumber(L, dmin);
            return 1;
        }


        private static int math_max(LuaState L)
        {
            int n = lua_gettop(L);  /* number of arguments */
            lua_Number dmax = luaL_checknumber(L, 1);
            int i;
            for (i = 2; i <= n; i++)
            {
                lua_Number d = luaL_checknumber(L, i);
                if (d > dmax)
                    dmax = d;
            }
            lua_pushnumber(L, dmax);
            return 1;
        }

        static int factorial(int n)
        {
            if (n <= 1)
                return n;
            else
                return n * factorial(n - 1);
        }

        private static int math_factorial(LuaState L)
        {
            int num = luaL_checkint(L, 1);
            lua_pushnumber(L, factorial(num));
            return 1;
        }

        private static Random rng = new Random();

        private static int math_random(LuaState L)
        {
            /* the `%' avoids the (rare) case of r==1, and is needed also because on
               some systems (SunOS!) `rand()' may return a value larger than RAND_MAX */
            //lua_Number r = (lua_Number)(rng.Next()%RAND_MAX) / (lua_Number)RAND_MAX;
            lua_Number r = (lua_Number)rng.NextDouble();
            switch (lua_gettop(L))
            {  /* check number of arguments */
                case 0:
                    {  /* no arguments */
                        lua_pushnumber(L, r);  /* Number between 0 and 1 */
                        break;
                    }
                case 1:
                    {  /* only upper limit */
                        int u = luaL_checkint(L, 1);
                        luaL_argcheck(L, 1 <= u, 1, "interval is empty");
                        lua_pushnumber(L, Math.Floor(r * u) + 1);  /* int between 1 and `u' */
                        break;
                    }
                case 2:
                    {  /* lower and upper limits */
                        int l = luaL_checkint(L, 1);
                        int u = luaL_checkint(L, 2);
                        luaL_argcheck(L, l <= u, 2, "interval is empty");
                        lua_pushnumber(L, Math.Floor(r * (u - l + 1)) + l);  /* int between `l' and `u' */
                        break;
                    }
                default: return luaL_error(L, "wrong number of arguments");
            }
            return 1;
        }


        private static int math_randomseed(LuaState L)
        {
            //srand(luaL_checkint(L, 1));
            rng = new Random(luaL_checkint(L, 1));
            return 0;
        }


        private readonly static luaL_Reg[] mathlib = {
		  new luaL_Reg("abs",   math_abs),
		  new luaL_Reg("acos",  math_acos),
		  new luaL_Reg("asin",  math_asin),
		  new luaL_Reg("atan2", math_atan2),
		  new luaL_Reg("atan",  math_atan),
		  new luaL_Reg("ceil",  math_ceil),
		  new luaL_Reg("cosh",   math_cosh),
		  new luaL_Reg("cos",   math_cos),
		  new luaL_Reg("deg",   math_deg),
		  new luaL_Reg("exp",   math_exp),
		  new luaL_Reg("floor", math_floor),
		  new luaL_Reg("fmod",   math_fmod),
		  new luaL_Reg("frexp", math_frexp),
		  new luaL_Reg("ldexp", math_ldexp),
		  new luaL_Reg("log10", math_log10),
		  new luaL_Reg("log",   math_log),
		  new luaL_Reg("max",   math_max),
		  new luaL_Reg("min",   math_min),
		  new luaL_Reg("modf",   math_modf),
		  new luaL_Reg("pow",   math_pow),
		  new luaL_Reg("rad",   math_rad),
		  new luaL_Reg("random",     math_random),
		  new luaL_Reg("randomseed", math_randomseed),
		  new luaL_Reg("sinh",   math_sinh),
		  new luaL_Reg("sin",   math_sin),
		  new luaL_Reg("sqrt",  math_sqrt),
		  new luaL_Reg("tanh",   math_tanh),
		  new luaL_Reg("tan",   math_tan),
          new luaL_Reg("factorial", math_factorial),
		  new luaL_Reg(null, null)
		};


        /*
        ** Open math library
        */
        public static int luaopen_math(LuaState L)
        {
            luaL_register(L, LUA_MATHLIBNAME, mathlib);
            lua_pushnumber(L, PI);
            lua_setfield(L, -2, "pi");
            lua_pushnumber(L, HUGE_VAL);
            lua_setfield(L, -2, "huge");
#if LUA_COMPAT_MOD
            lua_getfield(L, -1, "fmod");
            lua_setfield(L, -2, "mod");
#endif
            return 1;
        }

    }
}
