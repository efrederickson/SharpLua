using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;

namespace SharpLua
{
    public partial class Refactoring
    {
        /// <summary>
        /// Todo: Check references to make sure that it is only used once or twice
        /// It isn't working correctly... 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static List<Tuple<Variable, Variable>> FindMisspelledVariables(Chunk c)
        {
            List<Tuple<Variable, Variable>> misspelled = new List<Tuple<Variable, Variable>>();
            List<Variable> vars = c.Scope.GetAllVariables();
            List<Tuple<Variable, Variable>> scanned = new List<Tuple<Variable, Variable>>();
            //Console.WriteLine(vars.Count);
            Func<Variable, Variable, bool> get = new Func<Variable, Variable, bool>(delegate(Variable a, Variable b)
                {
                    bool found = false;
                    foreach (Tuple<Variable, Variable> t in scanned)
                    {
                        if (t.Item1 == a)
                            found = t.Item2 == b;
                        else if (t.Item1 == b)
                            found = t.Item1 == a;

                        if (found)
                            return found;
                    }
                    return false;
                });

            foreach (Variable v in vars)
            {
                foreach (Variable v2 in vars)
                {
                    if (v != v2 && get(v, v2) == false)
                    {
                        if (v.Name != v2.Name)
                        {
                            string a = v.Name.ToLower().Trim();
                            string b = v2.Name.ToLower().Trim();
                            if (a == b)
                            {
                                misspelled.Add(new Tuple<Variable, Variable>(v, v2));
                            }
                            else
                            {
                                int fails = 0;
                                int beginFailing = -1;
                                for (int i = 0; i < a.Length; i++)
                                {
                                    if (b.Length <= i)
                                    {
                                        fails++;
                                        break;
                                    }
                                    char c2 = a[i];
                                    char c3 = b[i];
                                    if (char.ToLower(c2) == char.ToLower(c3))
                                    {
                                        // do nothing, i guess
                                    }
                                    else
                                    {
                                        if (fails == 0)
                                            beginFailing = i;
                                        fails++;
                                    }
                                }
                                if ((beginFailing == a.Length - fails && a.Length - fails > 0)
                                    || (beginFailing == b.Length - fails && b.Length - fails > 0)
                                    || fails < ((a.Length > 6 ? a.Length / 4 : 3))
                                    && Math.Abs(a.Length - b.Length) < 3
                                    && (a.Length > 1 && b.Length > 1))
                                {
                                    misspelled.Add(new Tuple<Variable, Variable>(v, v2));
                                }
                            }
                        }
                        scanned.Add(new Tuple<Variable, Variable>(v, v2));
                    }
                }
            }
            return misspelled;
        }
    }
}
