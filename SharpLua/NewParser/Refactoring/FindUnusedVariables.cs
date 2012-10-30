using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;

namespace SharpLua
{
    public partial class Refactoring
    {
        public static List<Variable> FindUnusedVariables(Chunk c)
        {
            List<Variable> unused = new List<Variable>();
            foreach (Variable v in c.Scope.GetAllVariables())
            {
                if (v.References == 0 // wait.. wut?
                    || v.References == 1)
                    unused.Add(v);
            }
            return unused;
        }

        public static List<Variable> FindUnusedLocalVariables(Chunk c)
        {
            List<Variable> unused = new List<Variable>();
            foreach (Variable v in c.Scope.GetAllVariables())
            {
                if (v.References == 0
                    || v.References == 1)
                    if (v.IsGlobal == false)
                        unused.Add(v);
            }
            return unused;
        }
    }
}
