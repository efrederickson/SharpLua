using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class Assignment : Statement
    {
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

        private static void SetKeyValue(LuaValue baseValue, LuaValue key, LuaValue value)
        {
            LuaValue newIndex = LuaNil.Nil;
            LuaTable table = baseValue as LuaTable;
            if (table != null)
            {
                if (table.ContainsKey(key))
                {
                    table.SetKeyValue(key, value);
                    return;
                }
                else
                {
                    if (table.MetaTable != null)
                    {
                        newIndex = table.MetaTable.GetValue("__newindex");
                    }

                    if (newIndex == LuaNil.Nil)
                    {
                        table.SetKeyValue(key, value);
                        return;
                    }
                }
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
