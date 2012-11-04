using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Ast
{
    public class Scope
    {
        public List<Variable> Locals = new List<Variable>();
        public List<Variable> Globals = new List<Variable>();
        public Scope Parent = null;
        Dictionary<string, Variable> oldLocalNamesMap = new Dictionary<string, Variable>();
        Dictionary<string, Variable> oldGlobalNamesMap = new Dictionary<string, Variable>();
        public List<Scope> Children = new List<Scope>();

        public Scope()
        {

        }

        public Scope(Scope parent)
        {
            Parent = parent;
            Parent.Children.Add(this);
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

            Locals.Add(new Variable { Name = name, References = 1 });

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
            if (oldLocalNamesMap.ContainsKey(name))
                return oldLocalNamesMap[name];
            Variable v = GetLocal(name);
            if (v != null)
                return v;
            return null;
        }

        void mapLocal(string name, Variable v)
        {
            if (oldLocalNamesMap.ContainsKey(name))
                oldLocalNamesMap[name] = v;
            else
                oldLocalNamesMap.Add(name, v);
        }

        /// <summary>
        /// If the variable was renamed using RenameVariable, this will find the old
        /// name and return the variable it WAS attached to. If it didn't find one, it
        /// will check GetGlobal for variables.
        /// </summary>
        /// <param name="name">Old variable name</param>
        /// <returns>The variable</returns>
        public Variable GetOldGlobal(string name)
        {
            if (oldGlobalNamesMap.ContainsKey(name))
                return oldGlobalNamesMap[name];
            Variable v = GetGlobal(name);
            if (v != null)
                return v;
            return null;
        }

        void mapGlobal(string name, Variable v)
        {
            if (oldGlobalNamesMap.ContainsKey(name))
                oldGlobalNamesMap[name] = v;
            else
                oldGlobalNamesMap.Add(name, v);
        }

        public Variable GetOldVariable(string name)
        {
            Variable v = GetOldLocal(name);
            if (v == null)
                return GetOldGlobal(name);
            return v;
        }

        public void RenameLocal(string oldName, string newName)
        {
            bool found = false;
            foreach (Variable v in Locals)
            {
                if (v.Name == oldName)
                {
                    v.Name = newName;
                    mapLocal(oldName, v);
                    found = true;
                    break;
                }
            }
            if (!found && Parent != null)
                Parent.RenameLocal(oldName, newName);
        }

        public void RenameLocal(Variable oldName, string newName)
        {
            RenameLocal(oldName.Name, newName);
        }

        public void RenameGlobal(string oldName, string newName)
        {
            bool found = false;
            foreach (Variable v in Globals)
            {
                if (v.Name == oldName)
                {
                    v.Name = newName;
                    mapGlobal(oldName, v);
                    found = true;
                    break;
                }
            }
            if (!found && Parent != null)
                Parent.RenameGlobal(oldName, newName);
        }

        public void RenameGlobal(Variable oldName, string newName)
        {
            RenameGlobal(oldName.Name, newName);
        }

        public void RenameVariable(string oldName, string newName)
        {
            if (GetLocal(oldName) == null)
                RenameGlobal(oldName, newName);
            else
                RenameLocal(oldName, newName);
        }

        public void RenameVariable(Variable oldName, string newName)
        {
            RenameVariable(oldName.Name, newName);
        }

        /// <summary>
        /// Returns ALL Variables (both local and global), including ones in parent scopes
        /// </summary>
        /// <returns></returns>
        public List<Variable> GetAllVariables()
        {
            List<Variable> v = getVars(true); // Down
            v.AddRange(getVars(false));       // up
            return v;
        }

        List<Variable> getVars(bool top)
        {
            List<Variable> v = new List<Variable>();
            if (top == true)
            {
                v.AddRange(Locals);
                v.AddRange(Globals);
            }
            if (Parent != null && top == false)
                v.AddRange(Parent.getVars(false));

            if (top)
            {
                foreach (Scope c in Children)
                {
                    v.AddRange(c.getVars(true));
                }
            }
            return v;
        }

        public Variable CreateGlobal(string name)
        {
            Variable v = new Variable();
            v.Name = name;
            v.IsGlobal = true;
            Globals.Add(v);
            v.References = 1;
            return v;
        }

        public Variable GetGlobal(string name)
        {
            foreach (Variable v in Globals)
                if (v.Name == name)
                    return v;
            if (Parent != null)
                return Parent.GetGlobal(name);
            return null;
        }

        /// <summary>
        /// Returns either Local or Global variable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Variable GetVariable(string name)
        {
            Variable v = GetLocal(name);
            if (v != null)
                return v;
            v = GetGlobal(name);
            return v;
        }

        public void ObfuscateLocals(int recommendedMaxLength = 7)
        {
            Random r = new Random(DateTime.Now.Millisecond);

            string valid = "abcdefghijklmopqrstuvwxyz1234567890_";
            foreach (Variable v in Locals)
            {
                string newName = "";
                int length = r.Next(1, v.Name.Length > recommendedMaxLength ? recommendedMaxLength : v.Name.Length);
                for (int i = 0; i < length; i++)
                    newName += valid[r.Next(0, valid.Length - 1)];
                if (!char.IsLetter(newName[0]) && newName[0] != '_')
                    newName = "_" + newName;
                while (GetVariable(newName) != null)
                    newName += valid[r.Next(0, valid.Length - 1)];
                RenameLocal(v.Name, newName);
            }
        }
    }
}
