/*
 ** $Id: lstate.c,v 2.36.1.2 2008/01/03 15:20:39 roberto Exp $
 ** Global State
 ** See Copyright Notice in lua.h
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SharpLua
{
    using lu_byte = System.Byte;
    using lu_int32 = System.Int32;
    using lu_mem = System.UInt32;
    using TValue = Lua.lua_TValue;
    using StkId = Lua.lua_TValue;
    using ptrdiff_t = System.Int32;
    using Instruction = System.UInt32;

    public partial class Lua
    {
        /* table of globals */
        public static TValue gt(LuaState L) { return L.l_gt; }

        /* registry */
        public static TValue registry(LuaState L) { return G(L).l_registry; }


        /* extra stack space to handle TM calls and some other extras */
        public const int EXTRA_STACK = 5;


        public const int BASIC_CI_SIZE = 8;

        public const int BASIC_STACK_SIZE = (2 * LUA_MINSTACK);



        public class stringtable
        {
            public GCObject[] hash;
            public lu_int32 nuse;  /* number of elements */
            public int size;
        };


        /*
         ** informations about a call
         */
        public class CallInfo : ArrayElement
        {
            private CallInfo[] values = null;
            private int index = -1;

            public void set_index(int index)
            {
                this.index = index;
            }

            public void set_array(object array)
            {
                this.values = (CallInfo[])array;
                Debug.Assert(this.values != null);
            }

            public CallInfo this[int offset]
            {
                get { return values[index + offset]; }
            }

            public static CallInfo operator +(CallInfo value, int offset)
            {
                return value.values[value.index + offset];
            }

            public static CallInfo operator -(CallInfo value, int offset)
            {
                return value.values[value.index - offset];
            }

            public static int operator -(CallInfo ci, CallInfo[] values)
            {
                Debug.Assert(ci.values == values);
                return ci.index;
            }

            public static int operator -(CallInfo ci1, CallInfo ci2)
            {
                Debug.Assert(ci1.values == ci2.values);
                return ci1.index - ci2.index;
            }

            public static bool operator <(CallInfo ci1, CallInfo ci2)
            {
                Debug.Assert(ci1.values == ci2.values);
                return ci1.index < ci2.index;
            }

            public static bool operator <=(CallInfo ci1, CallInfo ci2)
            {
                Debug.Assert(ci1.values == ci2.values);
                return ci1.index <= ci2.index;
            }

            public static bool operator >(CallInfo ci1, CallInfo ci2)
            {
                Debug.Assert(ci1.values == ci2.values);
                return ci1.index > ci2.index;
            }

            public static bool operator >=(CallInfo ci1, CallInfo ci2)
            {
                Debug.Assert(ci1.values == ci2.values);
                return ci1.index >= ci2.index;
            }

            public static CallInfo inc(ref CallInfo value)
            {
                value = value[1];
                return value[-1];
            }

            public static CallInfo dec(ref CallInfo value)
            {
                value = value[-1];
                return value[1];
            }

            public StkId base_;  /* base for this function */
            public StkId func;  /* function index in the stack */
            public StkId top;  /* top for this function */
            public InstructionPtr savedpc;
            public int nresults;  /* expected number of results from this function */
            public int tailcalls;  /* number of tail calls lost under this entry */
        };



        public static Closure curr_func(LuaState L) { return (clvalue(L.ci.func)); }
        public static Closure ci_func(CallInfo ci) { return (clvalue(ci.func)); }
        public static bool f_isLua(CallInfo ci) { return ci_func(ci).c.isC == 0; }
        public static bool isLua(CallInfo ci) { return (ttisfunction((ci).func) && f_isLua(ci)); }


        /*
         ** `global state', shared by all threads of this state
         */
        public class GlobalState
        {
            public stringtable strt = new stringtable(); /* hash table for strings */
            public lua_Alloc frealloc;  /* function to reallocate memory */
            public object ud;         /* auxiliary data to `frealloc' */
            public lu_byte currentwhite;
            public lu_byte gcstate;  /* state of garbage collector */
            public int sweepstrgc;  /* position of sweep in `strt' */
            public GCObject rootgc;  /* list of all collectable objects */
            public GCObjectRef sweepgc;  /* position of sweep in `rootgc' */
            public GCObject gray;  /* list of gray objects */
            public GCObject grayagain;  /* list of objects to be traversed atomically */
            public GCObject weak;  /* list of weak tables (to be cleared) */
            public GCObject tmudata;  /* last element of list of userdata to be GC */
            public Mbuffer buff = new Mbuffer();  /* temporary buffer for string concatentation */
            public lu_mem GCthreshold;
            public lu_mem totalbytes;  /* number of bytes currently allocated */
            public lu_mem estimate;  /* an estimate of number of bytes actually in use */
            public lu_mem gcdept;  /* how much GC is `behind schedule' */
            public int gcpause;  /* size of pause between successive GCs */
            public int gcstepmul;  /* GC `granularity' */
            public lua_CFunction panic;  /* to be called in unprotected errors */
            public TValue l_registry = new TValue();
            public LuaState mainthread;
            public UpVal uvhead = new UpVal();  /* head of double-linked list of all open upvalues */
            public Table[] mt = new Table[NUM_TAGS];  /* metatables for basic types */
            public TString[] tmname = new TString[(int)TMS.TM_N];  /* array with tag-method names */
        };

        /// <summary>
        /// 'Per thread' state
        /// </summary>
        public class LuaState : GCObject
        {
            // MWAHAHA WE HAZ INTERFACES THROUGH EPIK HAX

            internal lu_byte _status;
            public lu_byte status
            {
                get
                {
                    CheckInterface();
                    return _status;
                }
                set
                {
                    CheckInterface();
                    _status = value;
                }
            }

            internal StkId _top;
            public StkId top  /* first free slot in the stack */
            {
                get
                {
                    CheckInterface();
                    return _top;
                }
                set
                {
                    CheckInterface();
                    _top = value;
                }
            }

            internal StkId _base_;
            public StkId base_  /* base of current function */
            {
                get
                {
                    CheckInterface();
                    return _base_;
                }
                set
                {
                    CheckInterface();
                    _base_ = value;
                }
            }

            internal GlobalState _l_G;
            public GlobalState l_G
            {
                get
                {
                    CheckInterface();
                    return _l_G;
                }
                set
                {
                    CheckInterface();
                    _l_G = value;
                }
            }

            internal CallInfo _ci;
            public CallInfo ci  /* call info for current function */
            {
                get
                {
                    CheckInterface();
                    return _ci;
                }
                set
                {
                    CheckInterface();
                    _ci = value;
                }
            }

            internal InstructionPtr _savedpc = new InstructionPtr();
            public InstructionPtr savedpc  /* `savedpc' of current function */
            {
                get
                {
                    CheckInterface();
                    return _savedpc;
                }
                set
                {
                    CheckInterface();
                    _savedpc = value;
                }
            }

            internal StkId _stack_last;
            public StkId stack_last  /* last free slot in the stack */
            {
                get
                {
                    CheckInterface();
                    return _stack_last;
                }
                set
                {
                    CheckInterface();
                    _stack_last = value;
                }
            }

            internal StkId[] _stack;
            public StkId[] stack  /* stack base */
            {
                get
                {
                    CheckInterface();
                    return _stack;
                }
                set
                {
                    CheckInterface();
                    _stack = value;
                }
            }

            internal CallInfo _end_ci;
            public CallInfo end_ci  /* points after end of ci array*/
            {
                get
                {
                    CheckInterface();
                    return _end_ci;
                }
                set
                {
                    CheckInterface();
                    _end_ci = value;
                }
            }

            internal CallInfo[] _base_ci;
            public CallInfo[] base_ci  /* array of CallInfo's */
            {
                get
                {
                    CheckInterface();
                    return _base_ci;
                }
                set
                {
                    CheckInterface();
                    _base_ci = value;
                }
            }

            internal int _stacksize;
            public int stacksize
            {
                get
                {
                    CheckInterface();
                    return _stacksize;
                }
                set
                {
                    CheckInterface();
                    _stacksize = value;
                }
            }

            internal int _size_ci;
            public int size_ci  /* size of array `base_ci' */
            {
                get
                {
                    CheckInterface();
                    return _size_ci;
                }
                set
                {
                    CheckInterface();
                    _size_ci = value;
                }
            }

            internal ushort _nCcalls;
            public ushort nCcalls  /* number of nested C calls */
            {
                get
                {
                    CheckInterface();
                    return _nCcalls;
                }
                set
                {
                    CheckInterface();
                    _nCcalls = value;
                }
            }

            internal ushort _base_Ccalls;
            public ushort baseCcalls  /* nested C calls when resuming coroutine */
            {
                get
                {
                    CheckInterface();
                    return _base_Ccalls;
                }
                set
                {
                    CheckInterface();
                    _base_Ccalls = value;
                }
            }

            internal lu_byte _hookmask;
            public lu_byte hookmask
            {
                get
                {
                    CheckInterface();
                    return _hookmask;
                }
                set
                {
                    CheckInterface();
                    _hookmask = value;
                }
            }

            internal lu_byte _allowhook;
            public lu_byte allowhook
            {
                get
                {
                    CheckInterface();
                    return _allowhook;
                }
                set
                {
                    CheckInterface();
                    _allowhook = value;
                }
            }

            internal int _basehookcount;
            public int basehookcount
            {
                get
                {
                    CheckInterface();
                    return _basehookcount;
                }
                set
                {
                    CheckInterface();
                    _basehookcount = value;
                }
            }

            internal int _hookcount;
            public int hookcount
            {
                get
                {
                    CheckInterface();
                    return _hookcount;
                }
                set
                {
                    CheckInterface();
                    _hookcount = value;
                }
            }

            internal lua_Hook _hook;
            public lua_Hook hook
            {
                get
                {
                    CheckInterface();
                    return _hook;
                }
                set
                {
                    CheckInterface();
                    _hook = value;
                }
            }

            internal TValue _l_gt = new TValue();
            public TValue l_gt  /* table of globals */
            {
                get
                {
                    CheckInterface();
                    return _l_gt;
                }
                set
                {
                    CheckInterface();
                    _l_gt = value;
                }
            }

            internal TValue _env = new TValue();
            public TValue env  /* temporary place for environments */
            {
                get
                {
                    CheckInterface();
                    return _env;
                }
                set
                {
                    CheckInterface();
                    _env = value;
                }
            }

            internal GCObject _openupval;
            public GCObject openupval  /* list of open upvalues in this stack */
            {
                get
                {
                    CheckInterface();
                    return _openupval;
                }
                set
                {
                    CheckInterface();
                    _openupval = value;
                }
            }

            internal GCObject _gclist;
            public GCObject gclist
            {
                get
                {
                    CheckInterface();
                    return _gclist;
                }
                set
                {
                    CheckInterface();
                    _gclist = value;
                }
            }

            internal lua_longjmp _errorJmp;
            public lua_longjmp errorJmp  /* current error recover point */
            {
                get
                {
                    CheckInterface();
                    return _errorJmp;
                }
                set
                {
                    CheckInterface();
                    _errorJmp = value;
                }
            }

            internal ptrdiff_t _errfunc;
            public ptrdiff_t errfunc  /* current error handling function (stack index) */
            {
                get
                {
                    CheckInterface();
                    return _errfunc;
                }
                set
                {
                    CheckInterface();
                    _errfunc = value;
                }
            }

            LuaInterface _interface;
            public LuaInterface Interface
            {
                get
                {
                    if (_interface == null)
                        _interface = new LuaInterface(this);
                    return _interface;
                }
            }

            public LuaState()
            {
                // Creates all sorts of issues...
                //initializing = true;
                //_interface = new LuaInterface(this);
                //initializing = false;
            }

            public LuaState(LuaInterface i)
            {
                _interface = i;
            }

            /// <summary>
            /// Set a LuaInterface to use with this LuaState. Throws an exception if there
            /// already is one.
            /// </summary>
            /// <param name="li"></param>
            public void SetInterface(LuaInterface li)
            {
                if (_interface == null)
                    _interface = li;
                else
                    throw new Exception("A LuaInterface is already attached to this LuaState");
            }

            internal bool initializing = false;

            public void CheckInterface()
            {
                if (initializing == false)
                {
                    if (_interface == null)
                    {
                        // STILL CAUSING ISSUES :O

                        //StkId top = this._top;
                        //_interface = new LuaInterface(this);
                        //lua_settop(this, top);
                    }
                }
            }
        };


        public static GlobalState G(LuaState L) { return L.l_G; }
        public static void G_set(LuaState L, GlobalState s) { L.l_G = s; }


        /*
         ** Union of all collectable objects (not a union anymore in the C# port)
         */
        public class GCObject : GCheader, ArrayElement
        {
            public void set_index(int index)
            {
                //this.index = index;
            }

            public void set_array(object array)
            {
                //this.values = (GCObject[])array;
                //Debug.Assert(this.values != null);
            }

            public GCheader gch { get { return (GCheader)this; } }
            public TString ts { get { return (TString)this; } }
            public Udata u { get { return (Udata)this; } }
            public Closure cl { get { return (Closure)this; } }
            public Table h { get { return (Table)this; } }
            public Proto p { get { return (Proto)this; } }
            public UpVal uv { get { return (UpVal)this; } }
            public LuaState th { get { return (LuaState)this; } }
        };

        /*	this interface and is used for implementing GCObject references,
            it's used to emulate the behaviour of a C-style GCObject **
         */
        public interface GCObjectRef
        {
            void set(GCObject value);
            GCObject get();
        }

        public class ArrayRef : GCObjectRef, ArrayElement
        {
            public ArrayRef()
            {
                this.array_elements = null;
                this.array_index = 0;
                this.vals = null;
                this.index = 0;
            }
            public ArrayRef(GCObject[] array_elements, int array_index)
            {
                this.array_elements = array_elements;
                this.array_index = array_index;
                this.vals = null;
                this.index = 0;
            }
            public void set(GCObject value) { array_elements[array_index] = value; }
            public GCObject get() { return array_elements[array_index]; }

            public void set_index(int index)
            {
                this.index = index;
            }
            public void set_array(object vals)
            {
                // don't actually need this
                this.vals = (ArrayRef[])vals;
                Debug.Assert(this.vals != null);
            }

            // ArrayRef is used to reference GCObject objects in an array, the next two members
            // point to that array and the index of the GCObject element we are referencing
            GCObject[] array_elements;
            int array_index;

            // ArrayRef is itself stored in an array and derived from ArrayElement, the next
            // two members refer to itself i.e. the array and index of it's own instance.
            ArrayRef[] vals;
            int index;
        }

        public class OpenValRef : GCObjectRef
        {
            public OpenValRef(LuaState L) { this.L = L; }
            public void set(GCObject value) { this.L.openupval = value; }
            public GCObject get() { return this.L.openupval; }
            LuaState L;
        }

        public class RootGCRef : GCObjectRef
        {
            public RootGCRef(GlobalState g) { this.g = g; }
            public void set(GCObject value) { this.g.rootgc = value; }
            public GCObject get() { return this.g.rootgc; }
            GlobalState g;
        }

        public class NextRef : GCObjectRef
        {
            public NextRef(GCheader header) { this.header = header; }
            public void set(GCObject value) { this.header.next = value; }
            public GCObject get() { return this.header.next; }
            GCheader header;
        }


        /* macros to convert a GCObject into a specific value */
        public static TString rawgco2ts(GCObject o) { return (TString)check_exp(o.gch.tt == LUA_TSTRING, o.ts); }
        public static TString gco2ts(GCObject o) { return (TString)(rawgco2ts(o).tsv); }
        public static Udata rawgco2u(GCObject o) { return (Udata)check_exp(o.gch.tt == LUA_TUSERDATA, o.u); }
        public static Udata gco2u(GCObject o) { return (Udata)(rawgco2u(o).uv); }
        public static Closure gco2cl(GCObject o) { return (Closure)check_exp(o.gch.tt == LUA_TFUNCTION, o.cl); }
        public static Table gco2h(GCObject o) { return (Table)check_exp(o.gch.tt == LUA_TTABLE, o.h); }
        public static Proto gco2p(GCObject o) { return (Proto)check_exp(o.gch.tt == LUA_TPROTO, o.p); }
        public static UpVal gco2uv(GCObject o) { return (UpVal)check_exp(o.gch.tt == LUA_TUPVAL, o.uv); }
        public static UpVal ngcotouv(GCObject o) { return (UpVal)check_exp((o == null) || (o.gch.tt == LUA_TUPVAL), o.uv); }
        public static LuaState gco2th(GCObject o) { return (LuaState)check_exp(o.gch.tt == LUA_TTHREAD, o.th); }

        /* macro to convert any Lua object into a GCObject */
        public static GCObject obj2gco(object v) { return (GCObject)v; }


        public static int state_size(object x) { return Marshal.SizeOf(x) + LUAI_EXTRASPACE; }
        /*
        public static lu_byte fromstate(object l)
        {
            return (lu_byte)(l - LUAI_EXTRASPACE);
        }
         */
        public static LuaState tostate(object l)
        {
            Debug.Assert(LUAI_EXTRASPACE == 0, "LUAI_EXTRASPACE not supported");
            return (LuaState)l;
        }


        /*
         ** Main thread combines a thread state and the global state
         */
        public class LG : LuaState
        {
            public LuaState l { get { return this; } }
            public GlobalState g = new GlobalState();
        };



        private static void stack_init(LuaState L1, LuaState L)
        {
            /* initialize CallInfo array */
            L1.base_ci = luaM_newvector<CallInfo>(L, BASIC_CI_SIZE);
            L1.ci = L1.base_ci[0];
            L1.size_ci = BASIC_CI_SIZE;
            L1.end_ci = L1.base_ci[L1.size_ci - 1];
            /* initialize stack array */
            L1.stack = luaM_newvector<TValue>(L, BASIC_STACK_SIZE + EXTRA_STACK);
            L1.stacksize = BASIC_STACK_SIZE + EXTRA_STACK;
            L1._top = L1.stack[0];
            L1.stack_last = L1.stack[L1.stacksize - EXTRA_STACK - 1];
            /* initialize first ci */
            L1.ci.func = L1._top;
            setnilvalue(StkId.inc(ref L1._top));  /* `function' entry for this `ci' */
            L1.base_ = L1.ci.base_ = L1._top;
            L1.ci.top = L1._top + LUA_MINSTACK;
        }


        private static void freestack(LuaState L, LuaState L1)
        {
            luaM_freearray(L, L1.base_ci);
            luaM_freearray(L, L1.stack);
        }


        /*
         ** open parts that may cause memory-allocation errors
         */
        private static void f_luaopen(LuaState L, object ud)
        {
            GlobalState g = G(L);
            //UNUSED(ud);
            stack_init(L, L);  /* init stack */
            sethvalue(L, gt(L), luaH_new(L, 0, 2));  /* table of globals */
            sethvalue(L, registry(L), luaH_new(L, 0, 2));  /* registry */
            luaS_resize(L, MINSTRTABSIZE);  /* initial size of string table */
            luaT_init(L);
            luaX_init(L);
            luaS_fix(luaS_newliteral(L, MEMERRMSG));
            g.GCthreshold = 4 * g.totalbytes;
        }


        private static void preinit_state(LuaState L, GlobalState g)
        {
            G_set(L, g);
            L.stack = null;
            L.stacksize = 0;
            L.errorJmp = null;
            L.hook = null;
            L.hookmask = 0;
            L.basehookcount = 0;
            L.allowhook = 1;
            resethookcount(L);
            L.openupval = null;
            L.size_ci = 0;
            L.nCcalls = L.baseCcalls = 0;
            L.status = 0;
            L.base_ci = null;
            L.ci = null;
            L.savedpc = new InstructionPtr();
            L.errfunc = 0;
            setnilvalue(gt(L));
        }


        private static void close_state(LuaState L)
        {
            GlobalState g = G(L);
            luaF_close(L, L.stack[0]);  /* close all upvalues for this thread */
            luaC_freeall(L);  /* collect all objects */
            lua_assert(g.rootgc == obj2gco(L));
            lua_assert(g.strt.nuse == 0);
            luaM_freearray(L, G(L).strt.hash);
            luaZ_freebuffer(L, g.buff);
            freestack(L, L);
            lua_assert(g.totalbytes == GetUnmanagedSize(typeof(LG)));
            //g.frealloc(g.ud, fromstate(L), (uint)state_size(typeof(LG)), 0);
        }


        private static LuaState luaE_newthread(LuaState L)
        {
            //lua_State L1 = tostate(luaM_malloc(L, state_size(typeof(lua_State))));
            LuaState L1 = luaM_new<LuaState>(L);
            luaC_link(L, obj2gco(L1), LUA_TTHREAD);
            preinit_state(L1, G(L));
            stack_init(L1, L);  /* init stack */
            setobj2n(L, gt(L1), gt(L));  /* share table of globals */
            L1.hookmask = L.hookmask;
            L1.basehookcount = L.basehookcount;
            L1.hook = L.hook;
            resethookcount(L1);
            lua_assert(iswhite(obj2gco(L1)));
            return L1;
        }


        private static void luaE_freethread(LuaState L, LuaState L1)
        {
            luaF_close(L1, L1.stack[0]);  /* close all upvalues for this thread */
            lua_assert(L1.openupval == null);
            luai_userstatefree(L1);
            freestack(L, L1);
            //luaM_freemem(L, fromstate(L1));
        }


        public static LuaState lua_newstate(lua_Alloc f, object ud)
        {
            int i;
            LuaState L;
            GlobalState g;
            //object l = f(ud, null, 0, (uint)state_size(typeof(LG)));
            object l = f(typeof(LG));
            if (l == null) return null;
            L = tostate(l);
            ((LuaState)L).initializing = true;
            g = (L as LG).g;
            L.next = null;
            L.tt = LUA_TTHREAD;
            g.currentwhite = (lu_byte)bit2mask(WHITE0BIT, FIXEDBIT);
            L.marked = luaC_white(g);
            lu_byte marked = L.marked;	// can't pass properties in as ref
            set2bits(ref marked, FIXEDBIT, SFIXEDBIT);
            L.marked = marked;
            preinit_state(L, g);
            g.frealloc = f;
            g.ud = ud;
            g.mainthread = L;
            g.uvhead.u.l.prev = g.uvhead;
            g.uvhead.u.l.next = g.uvhead;
            g.GCthreshold = 0;  /* mark it as unfinished state */
            g.strt.size = 0;
            g.strt.nuse = 0;
            g.strt.hash = null;
            setnilvalue(registry(L));
            luaZ_initbuffer(L, g.buff);
            g.panic = null;
            g.gcstate = GCSpause;
            g.rootgc = obj2gco(L);
            g.sweepstrgc = 0;
            g.sweepgc = new RootGCRef(g);
            g.gray = null;
            g.grayagain = null;
            g.weak = null;
            g.tmudata = null;
            g.totalbytes = (uint)GetUnmanagedSize(typeof(LG));
            g.gcpause = LUAI_GCPAUSE;
            g.gcstepmul = LUAI_GCMUL;
            g.gcdept = 0;
            for (i = 0; i < NUM_TAGS; i++) g.mt[i] = null;
            if (luaD_rawrunprotected(L, f_luaopen, null) != 0)
            {
                /* memory allocation error: free partial state */
                close_state(L);
                L = null;
            }
            else
                luai_userstateopen(L);
            if (L != null)
                ((LuaState)L).initializing = false;
            return L;
        }


        private static void callallgcTM(LuaState L, object ud)
        {
            //UNUSED(ud);
            luaC_callGCTM(L);  /* call GC metamethods for all udata */
        }


        public static void lua_close(LuaState L)
        {
            L = G(L).mainthread;  /* only the main thread can be closed */
            lua_lock(L);
            luaF_close(L, L.stack[0]);  /* close all upvalues for this thread */
            luaC_separateudata(L, 1);  /* separate udata that have GC metamethods */
            L.errfunc = 0;  /* no error function during GC metamethods */
            do
            {  /* repeat until no more errors */
                L.ci = L.base_ci[0];
                L.base_ = L._top = L.ci.base_;
                L.nCcalls = L.baseCcalls = 0;
            } while (luaD_rawrunprotected(L, callallgcTM, null) != 0);
            lua_assert(G(L).tmudata == null);
            luai_userstateclose(L);
            close_state(L);
        }

    }
}
