using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class ForInStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaValue[] values = this.ExprList.ConvertAll(expr => expr.Evaluate(enviroment)).ToArray();
            LuaValue[] neatValues = LuaMultiValue.UnWrapLuaValues(values);

            LuaFunction func = neatValues[0] as LuaFunction;
            LuaValue state = neatValues[1];
            LuaValue loopVar = neatValues[2];

            var table = new LuaTable(enviroment);
            this.Body.Enviroment = table;

            while (true)
            {
                LuaValue result = func.Invoke(new LuaValue[] { state, loopVar });
                LuaMultiValue multiValue = result as LuaMultiValue;

                if (multiValue != null)
                {
                    neatValues = LuaMultiValue.UnWrapLuaValues(multiValue.Values);
                    loopVar = neatValues[0];

                    for (int i = 0; i < Math.Min(this.NameList.Count, neatValues.Length); i++)
                    {
                        table.SetNameValue(this.NameList[i], neatValues[i]);
                    }
                }
                else
                {
                    loopVar = result;
                    table.SetNameValue(this.NameList[0], result);
                }

                if (loopVar == LuaNil.Nil)
                {
                    break;
                }

                var returnValue = this.Body.Execute(out isBreak);
                if (returnValue != null || isBreak == true)
                {
                    isBreak = false;
                    return returnValue;
                }
            }

            isBreak = false;
            return null;
        }
    }
}
