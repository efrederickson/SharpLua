using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua;

namespace SharpLua.Compiler
{
    using BinOpr = Lua.BinOpr;
    using BlockCnt = Lua.BlockCnt;
    using CharPtr = Lua.CharPtr;
    using expdesc = Lua.expdesc;
    using expkind = Lua.expkind;
    using FuncState = Lua.FuncState;
    using LexState = Lua.LexState;
    using LocVar = Lua.LocVar;
    using lu_byte = System.Byte;
    using LuaState = Lua.LuaState;
    using Mbuffer = Lua.Mbuffer;
    using OpCode = Lua.OpCode;
    using TValue = Lua.lua_TValue;
    using Proto = Lua.Proto;
    using LHS_assign = Lua.LHS_assign;
    using RESERVED = Lua.RESERVED;
    using TString = Lua.TString;
    using UnOpr = Lua.UnOpr;
    using ZIO = Lua.Zio;
    using ConsControl = Lua.ConsControl;

    public class Compiler
    {
        FuncState currentFunc = null;

        string ChunkName;
        LuaState L = Lua.luaL_newstate();
        public Compiler() { }

        void DoExpr(Expression e, expdesc v)
        {
            if (e is AnonymousFunctionExpr) // function() ... end
            {
                body(e as AnonymousFunctionExpr, v, 0, 0);
                return;
            }
            else if (e is BinOpExpr)
            {
                BinOpExpr b = e as BinOpExpr;
                expdesc v2 = new expdesc();
                BinOpr op = getbinopr(b.Op);
                DoExpr(b.Lhs, v);
                Lua.luaK_infix(currentFunc, op, v);
                DoExpr(b.Rhs, v2);
                Lua.luaK_posfix(currentFunc, op, v, v2);
                return;
            }
            else if (e is BoolExpr)
            {
                if (((BoolExpr)e).Value)
                    init_exp(v, expkind.VTRUE, 0);
                else
                    init_exp(v, expkind.VFALSE, 0);
                return;
            }
            else if (e is CallExpr)//&& (!(e is StringCallExpr) && !(e is TableCallExpr)))
            {
                DoExpr((e as CallExpr).Base, v);
                Lua.luaK_exp2nextreg(currentFunc, v);
                funcargs(e as CallExpr, v);
                return;
            }
            else if (e is StringCallExpr)
            {
                DoExpr((e as CallExpr).Base, v);
                Lua.luaK_exp2nextreg(currentFunc, v);
                funcargs(e as CallExpr, v);
                return;
            }
            else if (e is TableCallExpr)
            {
                DoExpr((e as CallExpr).Base, v);
                Lua.luaK_exp2nextreg(currentFunc, v);
                funcargs(e as CallExpr, v);
                return;
            }
            else if (e is IndexExpr)
            {
                IndexExpr ie = e as IndexExpr;
                expdesc key = new expdesc();
                Lua.luaK_exp2anyreg(currentFunc, v);
                //yindex(ls, key);
                DoExpr(ie.Index, key);
                Lua.luaK_indexed(currentFunc, v, key);
                return;
            }
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
            {
                // I
                // AM
                // HOGGING
                // SCREEN
                // SPACE
                // SO
                // THAT
                // I 
                // GET
                // IMPLEMENTED
            }
            else if (e is MemberExpr)
            {
                MemberExpr me = e as MemberExpr;
                DoExpr(me.Base, v);
                if (me.Indexer == ".")
                {
                    expdesc key = new expdesc();
                    Lua.luaK_exp2anyreg(currentFunc, v);
                    checkname(me.Ident, key);
                    Lua.luaK_indexed(currentFunc, v, key);
                }
                else if (me.Indexer == ":")
                {
                    expdesc key = new expdesc();
                    checkname(me.Ident, key);
                    Lua.luaK_self(currentFunc, v, key);
                    //funcargs(v);

                }
                else
                    throw new LuaSourceException(0, 0, "Unknown indexer: " + me.Indexer);
                return;
            }
            else if (e is NilExpr)
            {
                init_exp(v, expkind.VNIL, 0);
                return;
            }
            else if (e is NumberExpr)
            {
                NumberExpr n = e as NumberExpr;
                init_exp(v, expkind.VKNUM, 0);
                double result;
                Lua.luaO_str2d(n.Value, out result);
                v.u.nval = result;
                return;
            }
            else if (e is StringExpr)
            {
                string actual = Unescaper.Unescape((e as StringExpr).Value);
                init_exp(v, expkind.VK, Lua.luaK_stringK(currentFunc, new TString(new CharPtr(actual))));
                return;
            }
            else if (e is TableConstructorExpr)
            {
                constructor(e as TableConstructorExpr, v);
                return;
            }
            else if (e is UnOpExpr)
            {
                UnOpExpr uoe = e as UnOpExpr;
                UnOpr uop = new UnOpr();
                uop = getunopr(uoe.Op);
                Lua.luaK_prefix(currentFunc, uop, v);
                return;
            }
            else if (e is VarargExpr)
            {
                check_condition(currentFunc.f.is_vararg != 0,
                                "cannot use " + Lua.LUA_QL("...") + " outside a vararg function");
                currentFunc.f.is_vararg &= unchecked((lu_byte)(~Lua.VARARG_NEEDSARG));  /* don't need 'arg' */
                init_exp(v, expkind.VVARARG, Lua.luaK_codeABC(currentFunc, OpCode.OP_VARARG, 0, 1, 0));
                return;
            }
            else if (e is VariableExpression)
            {
                TString varname = new TString(new CharPtr(((VariableExpression)e).Var.Name));
                if (singlevaraux(currentFunc, varname, v, 1) == expkind.VGLOBAL)
                    v.u.s.info = Lua.luaK_stringK(currentFunc, varname);  /* info points to global name */
                return;
            }

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }

        void DoStatement(Statement s)
        {
            if (s is AssignmentStatement && !(s is AugmentedAssignmentStatement))
            {
                AssignmentStatement a = s as AssignmentStatement;
                if (a.IsLocal)
                {
                    int nvars = 0;
                    int nexps;
                    expdesc e = new expdesc();
                    foreach (Expression v in a.Lhs)
                        new_localvar(new TString(new CharPtr(((VariableExpression)v).Var.Name)), nvars++);
                    if (a.Rhs.Count > 0)
                    {
                        nexps = explist1(a.Rhs, e);
                    }
                    else
                    {
                        e.k = expkind.VVOID;
                        nexps = 0;
                    }
                    adjust_assign(nvars, nexps, e);
                    adjustlocalvars(nvars);
                }
                else
                {
                    int nvars = a.Lhs.Count;
                    expdesc e = new expdesc();
                    List<LHS_assign> asns = new List<Lua.LHS_assign>();
                    LHS_assign lh = null;
                    foreach (Expression expr in a.Lhs)
                    {
                        LHS_assign nv = new Lua.LHS_assign();
                        DoExpr(expr, nv.v);
                        luaY_checklimit(currentFunc, nvars, Lua.LUAI_MAXCCALLS - L.nCcalls,
                                        "variables in assignment");
                        nv.prev = asns.Count > 0 ? asns[asns.Count - 1] : null;
                        lh = nv;
                    }


                    {  /* assignment . `=' explist1 */
                        int nexps;
                        nexps = explist1(a.Rhs, e);
                        if (nexps != nvars)
                        {
                            adjust_assign(nvars, nexps, e);
                            if (nexps > nvars)
                                currentFunc.freereg -= nexps - nvars;  /* remove extra values */
                        }
                        else
                        {
                            Lua.luaK_setoneret(currentFunc, e);  /* close last expression */
                            Lua.luaK_storevar(currentFunc, lh.v, e);
                            return;  /* avoid default */
                        }
                    }
                    init_exp(e, expkind.VNONRELOC, currentFunc.freereg - 1);  /* default assignment */
                    Lua.luaK_storevar(currentFunc, lh.v, e);
                }
                return;
            }
            else if (s is AugmentedAssignmentStatement)
                ;
            else if (s is BreakStatement)
            {
                breakstat();
                return;
            }
            else if (s is ContinueStatement)
            {
                continuestat();
                return;
            }
            else if (s is CallStatement)
            {
                CallStatement cs = s as CallStatement;
                expdesc v = new expdesc();
                DoExpr(cs.Expression, v);
                return;
            }
            else if (s is DoStatement)
            {
                DoChunk(((DoStatement)s).Body);
                return;
            }
            else if (s is GenericForStatement)
            {
                GenericForStatement gf = s as GenericForStatement;

                expdesc e = new expdesc();
                int nvars = 0;
                int line;
                int base_ = currentFunc.freereg;
                /* create control variables */
                new_localvarliteral("(for generator)", nvars++);
                new_localvarliteral("(for state)", nvars++);
                new_localvarliteral("(for control)", nvars++);
                /* create declared variables */
                foreach (Variable v in gf.VariableList)
                    new_localvar(new TString(new CharPtr(v.Name)), nvars++);
                adjust_assign(3, explist1(gf.Generators, e), e);
                Lua.luaK_checkstack(currentFunc, 3);  /* extra space to call generator */
                forbody(gf.Body, base_, 0, nvars - 3, 0);

                return;
            }
            else if (s is NumericForStatement)
            {
                NumericForStatement nf = s as NumericForStatement;

                int base_ = currentFunc.freereg;
                new_localvarliteral("(for index)", 0);
                new_localvarliteral("(for limit)", 1);
                new_localvarliteral("(for step)", 2);
                new_localvar(new TString(new CharPtr(nf.Variable.Name)), 3);
                exp1(nf.Start);  /* initial value */
                exp1(nf.End);  /* limit */
                if (nf.Step != null)
                    exp1(nf.Step);  /* optional step */
                else
                {  /* default step = 1 */
                    Lua.luaK_codeABx(currentFunc, OpCode.OP_LOADK, currentFunc.freereg, Lua.luaK_numberK(currentFunc, 1));
                    Lua.luaK_reserveregs(currentFunc, 1);
                }
                forbody(nf.Body, base_, 0, 1, 1);
                return;
            }
            else if (s is FunctionStatement)
            {
                FunctionStatement f = s as FunctionStatement;
                if (f.IsLocal == false)
                {
                    int needself = 0;
                    expdesc v = new expdesc(), b = new expdesc();
                    {
                        Expression tmp = f.Name;
                        do
                        {
                            if (tmp is IndexExpr)
                            {
                                IndexExpr ie = tmp as IndexExpr;
                                tmp = ie.Index;
                            }
                            else if (tmp is MemberExpr)
                            {
                                MemberExpr me = tmp as MemberExpr;
                                if (me.Indexer == ":")
                                    needself = 1;
                                tmp = null;
                            }
                        } while (tmp != null);
                    }
                    DoExpr(f.Name, v);
                    body(f, b, needself, 0);
                    Lua.luaK_storevar(currentFunc, v, b);
                }
                else
                {
                    expdesc v = new expdesc(), b = new expdesc();
                    new_localvar(new TString(new CharPtr((f.Name as StringExpr).Value)), 0);
                    init_exp(v, expkind.VLOCAL, currentFunc.freereg);
                    Lua.luaK_reserveregs(currentFunc, 1);
                    adjustlocalvars(1);
                    body(f, b, 0, 0);
                    Lua.luaK_storevar(currentFunc, v, b);
                    /* debug information will only see the variable after this point! */
                    getlocvar(currentFunc, currentFunc.nactvar - 1).startpc = currentFunc.pc;
                }
                return;
            }
            else if (s is GotoStatement)
                ;
            else if (s is IfStmt)
            {
                IfStmt ifs = s as IfStmt;

                int flist = 0;
                int escapelist = Lua.NO_JUMP;
                bool first = true;
                bool elseBlock = false;
                foreach (SubIfStmt sis in ifs.Body)
                {
                    if (s is ElseIfStmt && first)
                    {
                        ElseIfStmt eis = s as ElseIfStmt;
                        flist = test_then_block(eis);
                    }
                    if (s is ElseIfStmt)
                    {
                        ElseIfStmt eis = s as ElseIfStmt;
                        Lua.luaK_concat(currentFunc, ref escapelist, Lua.luaK_jump(currentFunc));
                        Lua.luaK_patchtohere(currentFunc, flist);
                        flist = test_then_block(eis);
                    }
                    else if (s is ElseStmt)
                    {
                        Lua.luaK_concat(currentFunc, ref escapelist, Lua.luaK_jump(currentFunc));
                        Lua.luaK_patchtohere(currentFunc, flist);
                        DoChunk(((ElseStmt)s).Body);
                        elseBlock = true;
                    }

                    first = false;
                }
                if (elseBlock == false)
                    Lua.luaK_concat(currentFunc, ref escapelist, flist);

                Lua.luaK_patchtohere(currentFunc, escapelist);
                return;
            }
            else if (s is LabelStatement)
                ;
            else if (s is RepeatStatement)
            {
                int condexit;
                int repeat_init = Lua.luaK_getlabel(currentFunc);
                BlockCnt bl1 = new BlockCnt(), bl2 = new BlockCnt();
                enterblock(currentFunc, bl1, 1);  /* loop block */
                enterblock(currentFunc, bl2, 0);  /* scope block */
                DoChunk(((RepeatStatement)s).Body);
                Lua.luaK_patchtohere(currentFunc, bl1.continuelist);
                condexit = cond(((RepeatStatement)s).Condition);  /* read condition (inside scope block) */
                if (bl2.upval == 0)
                {  /* no upvalues? */
                    leaveblock(currentFunc);  /* finish scope */
                    Lua.luaK_patchlist(currentFunc, condexit, repeat_init);  /* close the loop */
                }
                else
                {  /* complete semantics when there are upvalues */
                    breakstat();  /* if condition then break */
                    Lua.luaK_patchtohere(currentFunc, condexit);  /* else... */
                    leaveblock(currentFunc);  /* finish scope... */
                    Lua.luaK_patchlist(currentFunc, Lua.luaK_jump(currentFunc), repeat_init);  /* and repeat */
                }
                leaveblock(currentFunc);  /* finish loop */
                return;
            }
            else if (s is ReturnStatement)
            {
                retstat(s as ReturnStatement);
                return;
            }
            else if (s is UsingStatement)
                ;
            else if (s is WhileStatement)
            {
                int whileinit;
                int condexit;
                BlockCnt bl = new BlockCnt();
                whileinit = Lua.luaK_getlabel(currentFunc);
                condexit = cond(((WhileStatement)s).Condition);
                enterblock(currentFunc, bl, 1);
                DoChunk(((WhileStatement)s).Body);
                Lua.luaK_patchlist(currentFunc, Lua.luaK_jump(currentFunc), whileinit);
                Lua.luaK_patchlist(currentFunc, bl.continuelist, whileinit);  /* continue goes to start, too */
                leaveblock(currentFunc);
                Lua.luaK_patchtohere(currentFunc, condexit);  /* false conditions finish the loop */
                return;
            }

            throw new NotImplementedException(s.GetType().Name + " is not implemented");
        }

        void DoChunk(Chunk c)
        {
            DoChunk(c.Body);
        }

        void DoChunk(List<Statement> ss)
        {
            foreach (Statement s in ss)
                DoStatement(s);

            Lua.lua_assert(currentFunc.f.maxstacksize >= currentFunc.freereg &&
                           currentFunc.freereg >= currentFunc.nactvar);
            currentFunc.freereg = currentFunc.nactvar;  /* free registers */
        }

        public Proto Compile(Chunk c, string name)
        {
            ChunkName = name;
            FuncState fs = new FuncState();
            open_func(fs);
            fs.f.is_vararg = Lua.VARARG_ISVARARG;
            DoChunk(c);
            close_func(fs);

            return fs.f;
        }

        void funcargs(CallExpr e, expdesc f)
        {
            expdesc args = new expdesc();
            int base_, nparams;
            int line = 0;

            if (!(e is StringCallExpr) && !(e is TableCallExpr))
            {  /* funcargs . `(' [ explist1 ] `)' */
                //if (line != ls.lastline)
                //    Lua.luaX_syntaxerror(ls, "ambiguous syntax (function call or new statement)");
                if (e.Arguments.Count == 0)
                    args.k = expkind.VVOID;
                else
                {
                    die
                    explist1(e.Arguments, args);
                    Lua.luaK_setmultret(currentFunc, args);
                }
            }
            else if (e is TableCallExpr)
            {  /* funcargs . constructor */
                constructor(e.Arguments[0] as TableConstructorExpr, args);
            }
            else if (e is StringCallExpr)
            {  /* funcargs . STRING */
                codestring(args, new TString(new CharPtr(Unescaper.Unescape((e.Arguments[0] as StringExpr).Value))));
            }

            Lua.lua_assert(f.k == expkind.VNONRELOC);
            base_ = f.u.s.info;  /* base_ register for call */
            if (hasmultret(args.k) != 0)
                nparams = Lua.LUA_MULTRET;  /* open call */
            else
            {
                if (args.k != expkind.VVOID)
                    Lua.luaK_exp2nextreg(currentFunc, args);  /* close last argument */
                nparams = currentFunc.freereg - (base_ + 1);
            }
            init_exp(f, expkind.VCALL, Lua.luaK_codeABC(currentFunc, OpCode.OP_CALL, base_, nparams + 1, 2));
            Lua.luaK_fixline(currentFunc, line);
            currentFunc.freereg = base_ + 1;  /* call remove function and arguments and leaves
									(unless changed) one result */
        }

        private static TString str_checkname(string s)
        {
            return new TString(new CharPtr(s));
        }

        void checkname(string s, expdesc e)
        {
            codestring(e, str_checkname(s));
        }

        expkind singlevaraux(FuncState fs, TString n, expdesc var, int base_)
        {
            if (fs == null)
            {  /* no more levels? */
                init_exp(var, expkind.VGLOBAL, Lua.NO_REG);  /* default is global variable */
                return expkind.VGLOBAL;
            }
            else
            {
                int v = searchvar(fs, n);  /* look up at current level */
                if (v >= 0)
                {
                    init_exp(var, expkind.VLOCAL, v);
                    if (base_ == 0)
                        markupval(fs, v);  /* local will be used as an upval */
                    return expkind.VLOCAL;
                }
                else
                {  /* not found at current level; try upper one */
                    if (singlevaraux(fs.prev, n, var, 0) == expkind.VGLOBAL)
                        return expkind.VGLOBAL;
                    var.u.s.info = indexupvalue(fs, n, var);  /* else was LOCAL or UPVAL */
                    var.k = expkind.VUPVAL;  /* upvalue in this level */
                    return expkind.VUPVAL;
                }
            }
        }

        int indexupvalue(FuncState fs, TString name, expdesc v)
        {
            int i;
            Proto f = fs.f;
            int oldsize = f.sizeupvalues;
            for (i = 0; i < f.nups; i++)
            {
                if ((int)fs.upvalues[i].k == (int)v.k && fs.upvalues[i].info == v.u.s.info)
                {
                    Lua.lua_assert(f.upvalues[i] == name);
                    return i;
                }
            }
            /* new one */
            luaY_checklimit(fs, f.nups + 1, Lua.LUAI_MAXUPVALUES, "upvalues");
            Lua.luaM_growvector(fs.L, ref f.upvalues, f.nups, ref f.sizeupvalues, Lua.MAX_INT, "");
            while (oldsize < f.sizeupvalues) f.upvalues[oldsize++] = null;
            f.upvalues[f.nups] = name;
            Lua.luaC_objbarrier(fs.L, f, name);
            Lua.lua_assert(v.k == expkind.VLOCAL || v.k == expkind.VUPVAL);
            fs.upvalues[f.nups].k = Lua.cast_byte(v.k);
            fs.upvalues[f.nups].info = Lua.cast_byte(v.u.s.info);
            return f.nups++;
        }

        int searchvar(FuncState fs, TString n)
        {
            int i;
            for (i = fs.nactvar - 1; i >= 0; i--)
            {
                if (n == getlocvar(fs, i).varname)
                    return i;
            }
            return -1;  /* not found */
        }

        void markupval(FuncState fs, int level)
        {
            BlockCnt bl = fs.bl;
            while ((bl != null) && bl.nactvar > level) bl = bl.previous;
            if (bl != null) bl.upval = 1;
        }

        void continuestat()
        {
            BlockCnt bl = currentFunc.bl;
            int upval = 0;
            while (bl != null && bl.isbreakable == 0)
            {
                upval |= bl.upval;
                bl = bl.previous;
            }
            if (bl == null)
                throw new LuaSourceException(0, 0, "no loop to continue");
            if (upval != 0)
                Lua.luaK_codeABC(currentFunc, OpCode.OP_CLOSE, bl.nactvar, 0, 0);
            Lua.luaK_concat(currentFunc, ref bl.continuelist, Lua.luaK_jump(currentFunc));
        }

        void breakstat()
        {
            BlockCnt bl = currentFunc.bl;
            int upval = 0;
            {
                upval |= bl.upval;
                bl = bl.previous;
            }
            if (bl == null)
                throw new LuaSourceException(0, 0, "no loop to break");
            if (upval != 0)
                Lua.luaK_codeABC(currentFunc, OpCode.OP_CLOSE, bl.nactvar, 0, 0);
            Lua.luaK_concat(currentFunc, ref bl.breaklist, Lua.luaK_jump(currentFunc));
        }

        void retstat(ReturnStatement rs)
        {
            expdesc e = new expdesc();
            int first, nret;  /* registers with returned values */
            if (rs.Arguments.Count == 0)
                first = nret = 0;  /* return no values */
            else
            {
                nret = explist1(rs.Arguments, e);  /* optional return values */
                if (hasmultret(e.k) != 0)
                {
                    Lua.luaK_setmultret(currentFunc, e);
                    if (e.k == expkind.VCALL && nret == 1)
                    {  /* tail call? */
                        Lua.SET_OPCODE(Lua.getcode(currentFunc, e), OpCode.OP_TAILCALL);
                        Lua.lua_assert(Lua.GETARG_A(Lua.getcode(currentFunc, e)) == currentFunc.nactvar);
                    }
                    first = currentFunc.nactvar;
                    nret = Lua.LUA_MULTRET;  /* return all values */
                }
                else
                {
                    if (nret == 1)  /* only one single value? */
                        first = Lua.luaK_exp2anyreg(currentFunc, e);
                    else
                    {
                        Lua.luaK_exp2nextreg(currentFunc, e);  /* values must go to the `stack' */
                        first = currentFunc.nactvar;  /* return all `active' values */
                        Lua.lua_assert(nret == currentFunc.freereg - first);
                    }
                }
            }
            Lua.luaK_ret(currentFunc, first, nret);
        }

        void adjust_assign(int nvars, int nexps, expdesc e)
        {
            int extra = nvars - nexps;
            if (hasmultret(e.k) != 0)
            {
                extra++;  /* includes call itself */
                if (extra < 0) extra = 0;
                Lua.luaK_setreturns(currentFunc, e, extra);  /* last exp. provides the difference */
                if (extra > 1) Lua.luaK_reserveregs(currentFunc, extra - 1);
            }
            else
            {
                if (e.k != expkind.VVOID)
                    Lua.luaK_exp2nextreg(currentFunc, e);  /* close last expression */
                if (extra > 0)
                {
                    int reg = currentFunc.freereg;
                    Lua.luaK_reserveregs(currentFunc, extra);
                    Lua.luaK_nil(currentFunc, reg, extra);
                }
            }
        }

        int explist1(List<Expression> exprs, expdesc v)
        {
            /* explist1 . expr { `,' expr } */
            //int n = 0;  /* at least one expression */
            foreach (Expression e in exprs)
            {
                Lua.luaK_exp2nextreg(currentFunc, v);
                DoExpr(e, v);
                //n++;
            }
            //return n;
            return exprs.Count;
        }

        void forbody(List<Statement> chunk, int base_, int line, int nvars, int isnum)
        {
            /* forbody . DO block */
            BlockCnt bl = new BlockCnt();
            FuncState fs = currentFunc;
            int prep, endfor;
            adjustlocalvars(3);  /* control variables */
            prep = (isnum != 0) ? Lua.luaK_codeAsBx(fs, OpCode.OP_FORPREP, base_, Lua.NO_JUMP) : Lua.luaK_jump(fs);
            enterblock(fs, bl, 0);  /* scope for declared variables */
            adjustlocalvars(nvars);
            Lua.luaK_reserveregs(fs, nvars);
            DoChunk(chunk);
            leaveblock(fs);  /* end of scope for declared variables */
            Lua.luaK_patchtohere(fs, prep);
            Lua.luaK_patchtohere(fs, bl.previous.continuelist);	/* continue, if any, jumps to here */
            endfor = (isnum != 0) ? Lua.luaK_codeAsBx(fs, OpCode.OP_FORLOOP, base_, Lua.NO_JUMP) :
                Lua.luaK_codeABC(fs, OpCode.OP_TFORLOOP, base_, 0, nvars);
            Lua.luaK_fixline(fs, line);  /* pretend that `OP_FOR' starts the loop */
            Lua.luaK_patchlist(fs, ((isnum != 0) ? endfor : Lua.luaK_jump(fs)), prep + 1);
        }

        int exp1(Expression expr)
        {
            expdesc e = new expdesc();
            int k;
            DoExpr(expr, e);
            k = (int)e.k;
            Lua.luaK_exp2nextreg(currentFunc, e);
            return k;
        }

        public static void check_condition(bool c, CharPtr msg)
        {
            if (!(c))
                throw new LuaSourceException(0, 0, msg);
        }

        private static void init_exp(expdesc e, expkind k, int i)
        {
            e.f = e.t = Lua.NO_JUMP;
            e.k = k;
            e.u.s.info = i;
        }

        void parlist(AnonymousFunctionExpr expr)
        {
            Proto f = currentFunc.f;
            int nparams = 0;
            f.is_vararg = 0;
#if IMPLICIT_VARARG
            bool wasvararg = false; // if the parlist contains a '...'
#endif
            foreach (Variable v in expr.Arguments)
            {
                new_localvar(new TString(new CharPtr(v.Name)), nparams++);
            }
            if (expr.IsVararg)
            {
#if IMPLICIT_VARARG
                wasvararg = true;
#endif
#if LUA_COMPAT_VARARG
                /* use `arg' as default name */
                new_localvarliteral("arg", nparams++);
                f.is_vararg = Lua.VARARG_HASARG | Lua.VARARG_NEEDSARG;
#endif
                f.is_vararg |= Lua.VARARG_ISVARARG;
            }
#if IMPLICIT_VARARG
            if (wasvararg == false)
            {
#if LUA_COMPAT_VARARG
                /* use `arg' as default name */
                new_localvarliteral("arg", nparams++);
                f.is_vararg = Lua.VARARG_HASARG | Lua.VARARG_NEEDSARG;
#else
				f.is_vararg = 0;
#endif

                f.is_vararg |= Lua.VARARG_ISVARARG;
            }
#endif
            adjustlocalvars(nparams);
            f.numparams = Lua.cast_byte(currentFunc.nactvar - (f.is_vararg & Lua.VARARG_HASARG));
            Lua.luaK_reserveregs(currentFunc, currentFunc.nactvar);  /* reserve register for parameters */
        }

        void parlist(FunctionStatement expr)
        {
            Proto f = currentFunc.f;
            int nparams = 0;
            f.is_vararg = 0;
#if IMPLICIT_VARARG
            bool wasvararg = false; // if the parlist contains a '...'
#endif
            foreach (Variable v in expr.Arguments)
            {
                new_localvar(new TString(new CharPtr(v.Name)), nparams++);
            }
            if (expr.IsVararg)
            {
#if IMPLICIT_VARARG
                wasvararg = true;
#endif
#if LUA_COMPAT_VARARG
                /* use `arg' as default name */
                new_localvarliteral("arg", nparams++);
                f.is_vararg = Lua.VARARG_HASARG | Lua.VARARG_NEEDSARG;
#endif
                f.is_vararg |= Lua.VARARG_ISVARARG;
            }
#if IMPLICIT_VARARG
            if (wasvararg == false)
            {
#if LUA_COMPAT_VARARG
                /* use `arg' as default name */
                new_localvarliteral("arg", nparams++);
                f.is_vararg = Lua.VARARG_HASARG | Lua.VARARG_NEEDSARG;
#else
				f.is_vararg = 0;
#endif

                f.is_vararg |= Lua.VARARG_ISVARARG;
            }
#endif
            adjustlocalvars(nparams);
            f.numparams = Lua.cast_byte(currentFunc.nactvar - (f.is_vararg & Lua.VARARG_HASARG));
            Lua.luaK_reserveregs(currentFunc, currentFunc.nactvar);  /* reserve register for parameters */
        }

        void pushclosure(FuncState func, expdesc v)
        {
            Proto f = currentFunc.f;
            int oldsize = f.sizep;
            int i;
            Lua.luaM_growvector(L, ref f.p, currentFunc.np, ref f.sizep,
                                Lua.MAXARG_Bx, "constant table overflow");
            while (oldsize < f.sizep) f.p[oldsize++] = null;
            f.p[currentFunc.np++] = func.f;
            Lua.luaC_objbarrier(L, f, func.f);
            init_exp(v, expkind.VRELOCABLE, Lua.luaK_codeABx(currentFunc, OpCode.OP_CLOSURE, 0, currentFunc.np - 1));
            for (i = 0; i < func.f.nups; i++)
            {
                OpCode o = ((int)func.upvalues[i].k == (int)expkind.VLOCAL) ? OpCode.OP_MOVE : OpCode.OP_GETUPVAL;
                Lua.luaK_codeABC(currentFunc, o, 0, func.upvalues[i].info, 0);
            }
        }

        void body(AnonymousFunctionExpr func, expdesc e, int needself, int line)
        {
            /* body .  `(' parlist `)' chunk END */
            FuncState new_fs = new FuncState();
            open_func(new_fs);
            new_fs.f.linedefined = line;
            if (needself != 0)
            {
                new_localvarliteral("self", 0);
                adjustlocalvars(1);
            }
            parlist(func);
            DoChunk(func.Body);
            close_func(new_fs);
            pushclosure(new_fs, e);
        }

        void body(FunctionStatement func, expdesc e, int needself, int line)
        {
            /* body .  `(' parlist `)' chunk END */
            FuncState new_fs = new FuncState();
            open_func(new_fs);
            new_fs.f.linedefined = line;
            if (needself != 0)
            {
                new_localvarliteral("self", 0);
                adjustlocalvars(1);
            }
            parlist(func);
            DoChunk(func.Body);
            close_func(new_fs);
            pushclosure(new_fs, e);
        }

        void new_localvarliteral(CharPtr v, int n)
        {
            new_localvar(luaX_newstring("" + v, (uint)(v.chars.Length - 1)), n);
        }

        int registerlocalvar(TString varname)
        {
            Proto f = currentFunc.f;
            int oldsize = f.sizelocvars;
            Lua.luaM_growvector(L, ref f.locvars, currentFunc.nlocvars, ref f.sizelocvars,
                                (int)Lua.SHRT_MAX, "too many local variables");
            while (oldsize < f.sizelocvars) f.locvars[oldsize++].varname = null;
            f.locvars[currentFunc.nlocvars].varname = varname;
            Lua.luaC_objbarrier(L, f, varname);
            return currentFunc.nlocvars++;
        }

        TString luaX_newstring(CharPtr str, uint l)
        {
            TString ts = Lua.luaS_newlstr(L, str, l);
            TValue o = Lua.luaH_setstr(L, currentFunc.h, ts);  /* entry for `str' */
            if (Lua.ttisnil(o))
            {
                Lua.setbvalue(o, 1);  /* make sure `str' will not be collected */
                Lua.luaC_checkGC(L);
            }

            return ts;
        }

        void new_localvar(TString name, int n)
        {
            luaY_checklimit(currentFunc, currentFunc.nactvar + n + 1, Lua.LUAI_MAXVARS, "local variables");
            currentFunc.actvar[currentFunc.nactvar + n] = (ushort)registerlocalvar(name);
        }

        void adjustlocalvars(int nvars)
        {
            currentFunc.nactvar = Lua.cast_byte(currentFunc.nactvar + nvars);
            for (; nvars != 0; nvars--)
            {
                getlocvar(currentFunc, currentFunc.nactvar - nvars).startpc = currentFunc.pc;
            }
        }

        private int cond(Expression e)
        {
            /* cond . exp */
            expdesc v = new expdesc();
            //expr(ls, v);  /* read condition */
            expr(e, v);
            if (v.k == expkind.VNIL)
                v.k = expkind.VFALSE;  /* 'falses' are all equal here */
            Lua.luaK_goiftrue(currentFunc, v);
            return v.f;
        }

        private void expr(Expression e, expdesc v)
        {
            //subexpr(e, v, 0);
            DoExpr(e, v);
        }

        private UnOpr getunopr(string op)
        {
            switch (op)
            {
                case "not": return UnOpr.OPR_NOT;
                case "-": return UnOpr.OPR_MINUS;
                case "#": return UnOpr.OPR_LEN;
                case "!": return UnOpr.OPR_NOT;
                case "~": throw new NotImplementedException(); //return UnOpr.OPR_BITNEGATE;
                default: return UnOpr.OPR_NOUNOPR;
            }
        }

        private BinOpr getbinopr(string op)
        {
            switch (op)
            {
                case "+": return BinOpr.OPR_ADD;
                case "-": return BinOpr.OPR_SUB;
                case "*": return BinOpr.OPR_MUL;
                case "/": return BinOpr.OPR_DIV;
                case "%": return BinOpr.OPR_MOD;
                case "^": return BinOpr.OPR_POW;
                case "..": return BinOpr.OPR_CONCAT;
                case "~=": return BinOpr.OPR_NE;
                case "==": return BinOpr.OPR_EQ;
                case "<": return BinOpr.OPR_LT;
                case "<=": return BinOpr.OPR_LE;
                case ">": return BinOpr.OPR_GT;
                case ">=": return BinOpr.OPR_GE;
                case "and": return BinOpr.OPR_AND;
                case "or": return BinOpr.OPR_OR;

                case ">>": throw new NotImplementedException(); //return BinOpr.OPR_RSHIFT;
                default: return BinOpr.OPR_NOBINOPR;
            }
        }

        void constructor(TableConstructorExpr ls, expdesc t)
        {
            int pc = Lua.luaK_codeABC(currentFunc, OpCode.OP_NEWTABLE, 0, 0, 0);
            ConsControl cc = new ConsControl();
            cc.na = cc.nh = cc.tostore = 0;
            cc.t = t;
            init_exp(t, expkind.VRELOCABLE, pc);
            init_exp(cc.v, expkind.VVOID, 0);  /* no value (yet) */
            Lua.luaK_exp2nextreg(currentFunc, t);  /* fix it at stack top (for gc) */
            foreach (Expression e in ls.EntryList)
            {
                Lua.lua_assert(cc.v.k == expkind.VVOID || cc.tostore > 0);
                closelistfield(currentFunc, cc);
                if (e is TableConstructorStringKeyExpr)
                {
                    recfield(e, cc);
                }
                else if (e is TableConstructorKeyExpr)
                {  /* constructor_item . recfield */
                    recfield(e, cc);
                }
                else if (e is TableConstructorValueExpr)
                {  /* constructor_part . listfield */
                    listfield(e, cc);
                }
                else if (e is TableConstructorNamedFunctionExpr)
                {
                    recfield(e, cc);
                }
            }
            lastlistfield(currentFunc, cc);
            Lua.SETARG_B(new InstructionPtr(currentFunc.f.code, pc), Lua.luaO_int2fb((uint)cc.na)); /* set initial array size */
            Lua.SETARG_C(new InstructionPtr(currentFunc.f.code, pc), Lua.luaO_int2fb((uint)cc.nh));  /* set initial table size */
        }

        void lastlistfield(FuncState fs, ConsControl cc)
        {
            if (cc.tostore == 0)
                return;
            if (hasmultret(cc.v.k) != 0)
            {
                Lua.luaK_setmultret(fs, cc.v);
                Lua.luaK_setlist(fs, cc.t.u.s.info, cc.na, Lua.LUA_MULTRET);
                cc.na--;  /* do not count last expression (unknown number of elements) */
            }
            else
            {
                if (cc.v.k != expkind.VVOID)
                    Lua.luaK_exp2nextreg(fs, cc.v);
                Lua.luaK_setlist(fs, cc.t.u.s.info, cc.na, cc.tostore);
            }
        }

        int hasmultret(expkind k)
        {
            return ((k) == expkind.VCALL || (k) == expkind.VVARARG) ? 1 : 0;
        }

        private static void errorlimit(FuncState fs, int limit, CharPtr what)
        {
            CharPtr msg = (fs.f.linedefined == 0) ?
                Lua.luaO_pushfstring(fs.L, "main function has more than %d %s", limit, what) :
                Lua.luaO_pushfstring(fs.L, "function at line %d has more than %d %s",
                                     fs.f.linedefined, limit, what);
            throw new LuaSourceException(0, 0, msg);
        }

        void luaY_checklimit(FuncState fs, int v, int l, CharPtr m)
        {
            if ((v) > (l))
                errorlimit(fs, l, m);
        }

        void listfield(Expression e, ConsControl cc)
        {
            if (e is TableConstructorValueExpr)
            {
                TableConstructorValueExpr tableV = e as TableConstructorValueExpr;
                DoExpr(tableV.Value, cc.v);
                luaY_checklimit(currentFunc, cc.na, Lua.MAX_INT, "items in a constructor");
                cc.na++;
                cc.tostore++;
            }
            else
                throw new NotImplementedException(e.GetType().Name);
        }

        void recfield(Expression e, ConsControl cc)
        {
            if (e is TableConstructorKeyExpr)
            {
                TableConstructorKeyExpr tcke = e as TableConstructorKeyExpr;

                int reg = currentFunc.freereg;
                expdesc key = new expdesc(), val = new expdesc();
                int rkkey;
                DoExpr(tcke.Key, key);
                cc.nh++;

                rkkey = Lua.luaK_exp2RK(currentFunc, key);
                DoExpr(tcke.Value, val);
                Lua.luaK_codeABC(currentFunc, OpCode.OP_SETTABLE, cc.t.u.s.info, rkkey, Lua.luaK_exp2RK(currentFunc, val));
                currentFunc.freereg = reg;  /* free registers */
            }
            else if (e is TableConstructorNamedFunctionExpr)
            {
                throw new NotImplementedException();
            }
            else if (e is TableConstructorStringKeyExpr)
            {
                TableConstructorStringKeyExpr tcke = e as TableConstructorStringKeyExpr;

                int reg = currentFunc.freereg;
                expdesc key = new expdesc(), val = new expdesc();
                int rkkey;
                DoExpr(new StringExpr(tcke.Key), key);
                cc.nh++;

                rkkey = Lua.luaK_exp2RK(currentFunc, key);
                DoExpr(tcke.Value, val);
                Lua.luaK_codeABC(currentFunc, OpCode.OP_SETTABLE, cc.t.u.s.info, rkkey, Lua.luaK_exp2RK(currentFunc, val));
                currentFunc.freereg = reg;  /* free registers */
            }
        }

        void codestring(expdesc e, TString s)
        {
            init_exp(e, expkind.VK, Lua.luaK_stringK(currentFunc, s));
        }

        void closelistfield(FuncState fs, ConsControl cc)
        {
            if (cc.v.k == expkind.VVOID) return;  /* there is no list item */
            Lua.luaK_exp2nextreg(fs, cc.v);
            cc.v.k = expkind.VVOID;
            if (cc.tostore == Lua.LFIELDS_PER_FLUSH)
            {
                Lua.luaK_setlist(fs, cc.t.u.s.info, cc.na, cc.tostore);  /* flush */
                cc.tostore = 0;  /* no more items pending */
            }
        }

        private void enterblock(FuncState fs, BlockCnt bl, lu_byte isbreakable)
        {
            bl.breaklist = Lua.NO_JUMP;
            bl.continuelist = Lua.NO_JUMP;
            bl.isbreakable = isbreakable;
            bl.nactvar = fs.nactvar;
            bl.upval = 0;
            bl.previous = fs.bl;
            fs.bl = bl;
            Lua.lua_assert(fs.freereg == fs.nactvar);
        }

        private void leaveblock(FuncState fs)
        {
            BlockCnt bl = fs.bl;
            fs.bl = bl.previous;
            removevars(fs, bl.nactvar);
            if (bl.upval != 0)
                Lua.luaK_codeABC(fs, OpCode.OP_CLOSE, bl.nactvar, 0, 0);
            /* a block either controls scope or breaks (never both) */
            Lua.lua_assert((bl.isbreakable == 0) || (bl.upval == 0));
            Lua.lua_assert(bl.nactvar == fs.nactvar);
            fs.freereg = fs.nactvar;  /* free registers */
            Lua.luaK_patchtohere(fs, bl.breaklist);
        }

        private int test_then_block(ElseIfStmt body)
        {
            int condexit;
            condexit = cond(body.Condition);
            DoChunk(body.Body);
            return condexit;
        }

        private void open_func(FuncState fs)
        {
            Proto f = Lua.luaF_newproto(L);
            fs.f = f;
            fs.prev = currentFunc;  /* linked list of funcstates */
            fs.L = L;
            currentFunc = fs;
            fs.pc = 0;
            fs.lasttarget = -1;
            fs.jpc = Lua.NO_JUMP;
            fs.freereg = 0;
            fs.nk = 0;
            fs.np = 0;
            fs.nlocvars = 0;
            fs.nactvar = 0;
            fs.ls = new Lua.LexState() { L = L, lastline = 0, linenumber = 0, };
            fs.bl = null;
            f.source = new TString(new CharPtr(ChunkName));
            f.maxstacksize = 2;  /* registers 0/1 are always valid */
            fs.h = Lua.luaH_new(L, 0, 0);
            /* anchor table of constants and prototype (to avoid being collected) */
            Lua.sethvalue2s(L, L._top, fs.h);
            Lua.incr_top(L);
            Lua.setptvalue2s(L, L._top, f);
            Lua.incr_top(L);
        }

        private void close_func(FuncState closing)
        {
            FuncState fs = closing;
            Proto f = fs.f;
            removevars(fs, 0);
            Lua.luaK_ret(fs, 0, 0);  /* final return */
            Lua.luaM_reallocvector(L, ref f.code, f.sizecode, fs.pc/*, typeof(Instruction)*/);
            f.sizecode = fs.pc;
            Lua.luaM_reallocvector(L, ref f.lineinfo, f.sizelineinfo, fs.pc/*, typeof(int)*/);
            f.sizelineinfo = fs.pc;
            Lua.luaM_reallocvector(L, ref f.k, f.sizek, fs.nk/*, TValue*/);
            f.sizek = fs.nk;
            Lua.luaM_reallocvector(L, ref f.p, f.sizep, fs.np/*, Proto*/);
            f.sizep = fs.np;
            for (int i = 0; i < f.p.Length; i++)
            {
                f.p[i].protos = f.p;
                f.p[i].index = i;
            }
            Lua.luaM_reallocvector(L, ref f.locvars, f.sizelocvars, fs.nlocvars/*, LocVar*/);
            f.sizelocvars = fs.nlocvars;
            Lua.luaM_reallocvector(L, ref f.upvalues, f.sizeupvalues, f.nups/*, TString*/);
            f.sizeupvalues = f.nups;
            Lua.lua_assert(Lua.luaG_checkcode(f));
            Lua.lua_assert(fs.bl == null);
            currentFunc = fs.prev;

            /* last token read was anchored in defunct function; must reanchor it */
            //if (fs != null)
            //	anchor_token(ls);

            L._top -= 2;  /* remove table and prototype from the stack */
        }

        private void removevars(FuncState fs, int tolevel)
        {
            while (fs.nactvar > tolevel)
                getlocvar(fs, --fs.nactvar).endpc = fs.pc;
        }

        public LocVar getlocvar(FuncState fs, int i)
        {
            return fs.f.locvars[fs.actvar[i]];
        }

        private static void anchor_token(LexState ls)
        {
            if (ls.t.token == (int)RESERVED.TK_NAME || ls.t.token == (int)RESERVED.TK_STRING)
            {
                TString ts = ls.t.seminfo.ts;
                Lua.luaX_newstring(ls, Lua.getstr(ts), ts.tsv.len);
            }
        }
    }

}
