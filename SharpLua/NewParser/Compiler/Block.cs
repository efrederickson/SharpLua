using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.LASM;

namespace SharpLua.Compiler
{
    class Block
    {
        public bool IsLoop = false;
        public Block PreviousBlock = null; // 

        public Chunk Chunk = null;

        public K2Reg K = null;
        public Var2Reg V = new Var2Reg();

        public Block()
        {
            Chunk = new Chunk();
            K = new K2Reg(this);
        }

        public Block(Block parent)
        {
            K = new K2Reg(this);
            //parent.PreviousBlock = this;
            this.PreviousBlock = parent;
            V = new Var2Reg(parent.V);
            Chunk = parent.Chunk;
        }

        public int regnum = 0;
        public int getreg()
        {
            //Console.WriteLine(regnum);
            return
                /*++*/
                regnum
                ++
                ;
        }

        public void CheckLocalName(string varname)
        {
            //if (V.has(varname))
            //{
            Local l = new Local(varname, 0, 0);
            if (Chunk.Locals.Any((L) => l.Name == varname))
                return;
            Chunk.Locals.Add(l);
            //}
        }
    }
}
