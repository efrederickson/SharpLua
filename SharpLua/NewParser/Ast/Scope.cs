using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast
{
    public class Scope
    {
        public List<Variable> Locals = new List<Variable>();
        public Scope Parent = null;
        Dictionary<string, Variable> oldNamesMap = new Dictionary<string, Variable>();

        public Scope()
        {

        }

        public Scope(Scope parent)
        {
            Parent = parent;
        }

        public void AddLocal(Variable v)
        {
            Locals.Add(v);
        }

        public Variable CreateLocal(string name)
        {
            Variable x = GetLocal(name);
            if (x != null)
                return x;

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
            if (Parent != null)
                return Parent.GetLocal(name);
            return null;
        }

        /// <summary>
        /// If the variable was renamed using RenameVariable, this will find the old
        /// name and return the variable it WAS attached to. If it didn't find one, it
        /// will check GetLocal for variables.
        /// </summary>
        /// <param name="name">Old variable name</param>
        /// <returns>The variable</returns>
        public Variable GetOldLocal(string name)
        {
            if (oldNamesMap.ContainsKey(name))
                return oldNamesMap[name];
            Variable v = GetLocal(name);
            if (v != null)
                return v;
            return null;
        }

        void map(string name, Variable v)
        {
            if (oldNamesMap.ContainsKey(name))
                oldNamesMap[name] = v;
            else
                oldNamesMap.Add(name, v);
        }

        public void RenameVariable(string oldName, string newName)
        {
            GetLocal(oldName).Name = newName;
            map(oldName, GetLocal(newName));
        }

        public void RenameVariable(Variable oldName, string newName)
        {
            string nm = oldName.Name;
            GetLocal(oldName.Name).Name = newName;
            map(nm, GetLocal(newName));
        }
    }
}
