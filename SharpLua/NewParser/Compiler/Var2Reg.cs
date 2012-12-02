using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Compiler
{
    class Var2Reg
    {
        public Dictionary<object, int> dic = new Dictionary<object, int>();

        public Var2Reg Parent = null;

        public int this[object o]
        {
            get
            {
                if (dic.ContainsKey(o))
                    return dic[o];
                else if (Parent != null && Parent.has(o))
                    return Parent[o];
                else
                {
                    dic.Add(o, dic.Count);
                    return dic[o];
                }
            }/*
            set
            {
                dic[o] = value;
            }*/
        }

        public bool has(object o)
        {
            return dic.ContainsKey(o) || (Parent != null && Parent.has(o));
        }

        public Var2Reg() { }
        public Var2Reg(Var2Reg par) { Parent = par; }
    }
}
