using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.LASM;

namespace SharpLua.Compiler
{
    class K2Reg
    {
        public Dictionary<object, int> dic = new Dictionary<object, int>();
        Block b = null;

        public int this[object o]
        {
            get
            {
                if (dic.ContainsKey(o))
                    return dic[o];
                else
                {
                    dic.Add(o, dic.Count);

                    Constant con = new Constant((ConstantType)(-1), null);
                    con.Value = o;
                    con.Number = dic.Count - 1;

                    if (o is string)
                    {
                        con.Type = ConstantType.String;
                    }
                    else if (o is double)
                        con.Type = ConstantType.Number;
                    else if (o is bool)
                        con.Type = ConstantType.Bool;
                    else if (o == null)
                    {
                        con.Type = ConstantType.Nil;
                    }
                    else
                        throw new Exception("Invalid constant type '" + o.GetType().ToString() + "'!");

                    b.Chunk.Constants.Add(con);

                    return dic[o];
                }
            }/*
            set
            {
                dic[o] = value;
            }*/
        }

        public K2Reg(Block b)
        {
            this.b = b;
        }

        public void Check(object o)
        {
            object tmp = this[o];
        }
    }
}
