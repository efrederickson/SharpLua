using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast
{
    public class Scope
    {
        public List<Variable> Locals = new List<Variable>();

        public void AddLocal(Variable v)
        {
            Locals.Add(v);
        }

        public Variable CreateLocal(string name)
        {
            Locals.Add(new Variable { Name = name });

            foreach (Variable v in Locals)
                if (v.Name == name)
                    return v;
            throw new Exception();
        }

        public Variable GetLocal(string name)
        {
            foreach (Variable v in Locals)
                if (v.Name == name)
                    return v;
            return null;
        }
    }
}
