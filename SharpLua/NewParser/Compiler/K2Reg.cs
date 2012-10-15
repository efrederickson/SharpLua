using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua;

namespace SharpLua.Compiler
{
    class K2Reg
    {
        Dictionary<Lua.Proto, Dictionary<Variable, int>> Addresses = new Dictionary<Lua.Proto, Dictionary<Variable, int>>();
        Dictionary<Lua.Proto, int> indexes = new Dictionary<Lua.Proto, int>();

        Lua.Proto _G = new Lua.Proto();

        void check(Lua.Proto s, Variable v, int index)
        {
            if (Addresses.ContainsKey(s) == false)
            {
                Addresses.Add(s, new Dictionary<Variable, int>());
            }
            if (Addresses[s].ContainsKey(v) == false)
            {
                Addresses[s].Add(v, index);
            }
        }

        int getOrCreate(Lua.Proto s, Variable v)
        {
            if (indexes.ContainsKey(s) == false)
            {
                indexes.Add(s, 0);
            }
            if (Addresses[s].ContainsKey(v) == false)
                check(s, v, indexes[s]++);
            else
                return Addresses[s][v];
            return Addresses[s][v];
        }

        public int ToReg(Lua.Proto p, Variable v)
        {
            return getOrCreate(p, v);
        }
    }
}
