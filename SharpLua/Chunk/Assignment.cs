using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// An assignment statement 
    /// </summary>
    public partial class Assignment : Statement
    {
        /// <summary>
        /// Runs the Statement
        /// </summary>
        /// <param name="enviroment">The environment to run in</param>
        /// <param name="isBreak">whether to break execution or not</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaValue[] values = this.ExprList.ConvertAll(expr => expr.Evaluate(enviroment)).ToArray();
            LuaValue[] neatValues = LuaMultiValue.UnWrapLuaValues(values);

            for (int i = 0; i < Math.Min(this.VarList.Count, neatValues.Length); i++)
            {
                Var var = this.VarList[i];

                if (var.Accesses.Count == 0)
                {
                    VarName varName = var.Base as VarName;

                    if (varName != null)
                    {
                        // inital creation, no metatable yet
                        SetKeyValue(enviroment, new LuaString(varName.Name), values[i]);
                        continue;
                    }
                }
                else
                {
                    LuaValue baseValue = var.Base.Evaluate(enviroment);

                    for (int j = 0; j < var.Accesses.Count - 1; j++)
                    {
                        Access access = var.Accesses[j];

                        baseValue = access.Evaluate(baseValue, enviroment);
                    }

                    Access lastAccess = var.Accesses[var.Accesses.Count - 1];

                    NameAccess nameAccess = lastAccess as NameAccess;
                    if (nameAccess != null)
                    {
                        SetKeyValue(baseValue, new LuaString(nameAccess.Name), values[i]);
                        continue;
                    }

                    KeyAccess keyAccess = lastAccess as KeyAccess;
                    if (lastAccess != null)
                    {
                        SetKeyValue(baseValue, keyAccess.Key.Evaluate(enviroment), values[i]);
                    }
                }
            }

            isBreak = false;
            return null;
        }
        
        /// <summary>
        /// Sets the assignment
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SetKeyValue(LuaValue baseValue, LuaValue key, LuaValue value)
        {
            LuaValue newIndex = LuaNil.Nil;
            LuaTable table = baseValue as LuaTable;
            if (table != null)
            {
                if (table.MetaTable != null)
                {
                    newIndex = table.MetaTable.GetValue("__newindex");
                    // to be finished at the end of this method
                }

                if (newIndex == LuaNil.Nil)
                {
                    table.SetKeyValue(key, value);
                    return;
                }
            }
            else if ((baseValue as LuaClass) != null)
            {
                LuaClass c = baseValue as LuaClass;
                // null checks (mainly for debugging)
                if (c.Self.MetaTable == null)
                    c.GenerateMetaTable();
                //throw new Exception("Class metatable is nil!");
                newIndex = c.Self.MetaTable.GetValue("__newindex");
                if (newIndex == LuaNil.Nil)
                    c.Self.SetKeyValue(key, value);
                else
                    (newIndex as LuaFunction).Invoke(new LuaValue[] { baseValue, key, value });
                return;
            }
            else
            {
                LuaUserdata userdata = baseValue as LuaUserdata;
                if (userdata != null)
                {
                    if (userdata.MetaTable != null)
                    {
                        newIndex = userdata.MetaTable.GetValue("__newindex");
                    }

                    if (newIndex == LuaNil.Nil)
                    {
                        throw new Exception("Assign field of userdata without __newindex defined.");
                    }
                }
            }
            
            LuaFunction func = newIndex as LuaFunction;
            if (func != null)
            {
                func.Invoke(new LuaValue[] { baseValue, key, value });
            }
            else
            {
                SetKeyValue(newIndex, key, value);
            }
        }
    }
}
