/*
** $Id: luac.c,v 1.54 2006/06/02 17:37:11 lhf Exp $
** Lua compiler (saves bytecodes to files; also list bytecodes)
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using SharpLua;

namespace SharpLua
{
    using Instruction = System.UInt32;

    public class Luac
    {
        //#include <errno.h>
        //#include <stdio.h>
        //#include <stdlib.h>
        //#include <string.h>

        //#define luac_c
        //#define LUA_CORE

        //#include "lua.h"
        //#include "lauxlib.h"

        //#include "ldo.h"
        //#include "lfunc.h"
        //#include "lmem.h"
        //#include "lobject.h"
        //#include "lopcodes.h"
        //#include "lstring.h"
        //#include "lundump.h"

        static Lua.CharPtr PROGNAME = "compiled";		/* default program name */
        static Lua.CharPtr OUTPUT = PROGNAME + ".sluac"; /* default output file */

        static int listing = 0;			/* list bytecodes? */
        static int dumping = 1;			/* dump bytecodes? */
        static int stripping = 0;			/* strip debug information? */
        static Lua.CharPtr Output = OUTPUT;	/* default output file name */
        static Lua.CharPtr output = Output;	/* actual output file name */
        static Lua.CharPtr progname = PROGNAME;	/* actual program name */

        static void fatal(Lua.CharPtr message)
        {
            Lua.fprintf(Lua.stderr, "%s: %s\n", progname, message);
            Environment.Exit(Lua.EXIT_FAILURE);
        }

        static void cannot(Lua.CharPtr what)
        {
            Lua.fprintf(Lua.stderr, "%s: cannot %s %s: %s\n", progname, what, output, Lua.strerror(Lua.errno()));
            Environment.Exit(Lua.EXIT_FAILURE);
        }

        static void usage(Lua.CharPtr message)
        {
            if (message[0] == '-')
                Lua.fprintf(Lua.stderr, "%s: unrecognized option " + Lua.LUA_QS + "\n", progname, message);
            else
                Lua.fprintf(Lua.stderr, "%s: %s\n", progname, message);
            Lua.fprintf(Lua.stderr,
            "usage: %s [options] [filenames].\n" +
            "Available options are:\n" +
            "  -        process stdin\n" +
            "  -l       list\n" +
            "  -o name  output to file " + Lua.LUA_QL("name") + " (default is \"%s\")\n" +
            "  -p       parse only\n" +
            "  -s       strip debug information\n" +
            "  -v       show version information\n" +
            "  --       stop handling options\n",
            progname, Output);
            Environment.Exit(Lua.EXIT_FAILURE);
        }

        //#define	IS(s)	(strcmp(argv[i],s)==0)

        static int doargs(int argc, string[] argv)
        {
            int i;
            int version = 0;
            if ((argv.Length > 0) && (argv[0] != "")) progname = argv[0];
            for (i = 1; i < argc; i++)
            {
                if (argv[i][0] != '-')			/* end of options; keep it */
                    break;
                else if (Lua.strcmp(argv[i], "--") == 0)			/* end of options; skip it */
                {
                    ++i;
                    if (version != 0) ++version;
                    break;
                }
                else if (Lua.strcmp(argv[i], "-") == 0)			/* end of options; use stdin */
                    break;
                else if (Lua.strcmp(argv[i], "-l") == 0)			/* list */
                    ++listing;
                else if (Lua.strcmp(argv[i], "-o") == 0)			/* output file */
                {
                    output = argv[++i];
                    if (output == null || (output[0] == 0)) usage(Lua.LUA_QL("-o") + " needs argument");
                    if (Lua.strcmp(argv[i], "-") == 0) output = null;
                }
                else if (Lua.strcmp(argv[i], "-p") == 0)			/* parse only */
                    dumping = 0;
                else if (Lua.strcmp(argv[i], "-s") == 0)			/* strip debug information */
                    stripping = 1;
                else if (Lua.strcmp(argv[i], "-v") == 0)			/* show version */
                    ++version;
                else					/* unknown option */
                    usage(argv[i]);
            }
            if (i == argc && ((listing != 0) || (dumping == 0)))
            {
                dumping = 0;
                argv[--i] = Output.ToString();
            }
            if (version != 0)
            {
                Lua.printf("%s  %s\n", Lua.LUA_RELEASE, Lua.LUA_COPYRIGHT);
                if (version == argc - 1) Environment.Exit(Lua.EXIT_SUCCESS);
            }
            return i;
        }

        static Lua.Proto toproto(Lua.LuaState L, int i)
        {
            return Lua.clvalue(L.top + (i)).l.p;
        }

        static Lua.Proto combine(Lua.LuaState L, int n)
        {
            if (n == 1)
                return toproto(L, -1);
            else
            {
                int i, pc;
                Lua.Proto f = Lua.luaF_newproto(L);
                Lua.setptvalue2s(L, L.top, f); Lua.incr_top(L);
                f.source = Lua.luaS_newliteral(L, "=(" + PROGNAME + ")");
                f.maxstacksize = 1;
                pc = 2 * n + 1;
                f.code = (Instruction[])Lua.luaM_newvector<Instruction>(L, pc);
                f.sizecode = pc;
                f.p = Lua.luaM_newvector<Lua.Proto>(L, n);
                f.sizep = n;
                pc = 0;
                for (i = 0; i < n; i++)
                {
                    f.p[i] = toproto(L, i - n - 1);
                    f.code[pc++] = (uint)Lua.CREATE_ABx(Lua.OpCode.OP_CLOSURE, 0, i);
                    f.code[pc++] = (uint)Lua.CREATE_ABC(Lua.OpCode.OP_CALL, 0, 1, 1);
                }
                f.code[pc++] = (uint)Lua.CREATE_ABC(Lua.OpCode.OP_RETURN, 0, 1, 0);
                return f;
            }
        }

        static int writer(Lua.LuaState L, Lua.CharPtr p, uint size, object u)
        {
            //UNUSED(L);
            return ((Lua.fwrite(p, (int)size, 1, (Stream)u) != 1) && (size != 0)) ? 1 : 0;
        }

        public class Smain
        {
            public int argc;
            public string[] argv;
        };

        static int pmain(Lua.LuaState L)
        {
            Smain s = (Smain)Lua.lua_touserdata(L, 1);
            int argc = s.argc;
            string[] argv = s.argv;
            Lua.Proto f;
            int i;
            if (Lua.lua_checkstack(L, argc) == 0) fatal("too many input files");
            for (i = 0; i < argc; i++)
            {
                Lua.CharPtr filename = (Lua.strcmp(argv[i], "-") == 0) ? null : argv[i];
                if (Lua.luaL_loadfile(L, filename) != 0) fatal(Lua.lua_tostring(L, -1));
            }
            f = combine(L, argc);
            if (listing != 0) Lua.luaU_print(f, (listing > 1) ? 1 : 0);
            if (dumping != 0)
            {
                Stream D = (output == null) ? Lua.stdout : Lua.fopen(output, "wb");
                if (D == null) cannot("open");
                Lua.lua_lock(L);
                Lua.luaU_dump(L, f, writer, D, stripping);
                Lua.lua_unlock(L);
                if (Lua.ferror(D) != 0) cannot("write");
                if (Lua.fclose(D) != 0) cannot("close");
            }
            return 0;
        }

        static int Main(string[] args)
        {
            // prepend the exe name to the arg list as it's done in C
            // so that we don't have to change any of the args indexing
            // code above
            List<string> newargs = new List<string>(args);
            newargs.Insert(0, Assembly.GetExecutingAssembly().Location);
            args = (string[])newargs.ToArray();

            Lua.LuaState L;
            Smain s = new Smain();
            int argc = args.Length;
            int i = doargs(argc, args);
            newargs.RemoveRange(0, i);
            argc -= i; args = (string[])newargs.ToArray();
            if (argc <= 0) usage("no input files given");
            L = Lua.lua_open();
            if (L == null) fatal("not enough memory for state");
            s.argc = argc;
            s.argv = args;
            if (Lua.lua_cpcall(L, pmain, s) != 0) fatal(Lua.lua_tostring(L, -1));
            Lua.lua_close(L);
            return Lua.EXIT_SUCCESS;
        }

    }
}
