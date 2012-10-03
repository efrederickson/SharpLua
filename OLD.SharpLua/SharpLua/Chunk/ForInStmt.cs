using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A for/in loop
    /// </summary>
    [Serializable()]
    public partial class ForInStmt : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">Runs in the given environment</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaValue[] values = this.ExprList.ConvertAll(expr => expr.Evaluate(enviroment)).ToArray();
            LuaValue[] neatValues = LuaMultiValue.UnWrapLuaValues(values);
            
            if (neatValues.Length < 3) //probably LuaUserdata. Literal will also fail...
            {
                return ExecuteAlternative(enviroment, out isBreak);
            }
            
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
        private LuaValue ExecuteAlternative(LuaTable enviroment, out bool isBreak)
        {
            LuaValue returnValue;
            LuaValue[] values = this.ExprList.ConvertAll(expr => expr.Evaluate(enviroment)).ToArray();
            LuaValue[] neatValues = LuaMultiValue.UnWrapLuaValues(values);
            
            LuaValue state = neatValues[0];
            
            LuaTable table = new LuaTable(enviroment);
            this.Body.Enviroment = table;
            System.Collections.IDictionary dict = state.Value as System.Collections.IDictionary;
            System.Collections.IEnumerable ie = state.Value as System.Collections.IEnumerable;
            if (dict != null)
            {
                foreach (object key in dict.Keys)
                {
                    //for (int i = 0; i < this.NameList.Count; i++)
                    //{
                    //table.SetNameValue(this.NameList[i], ObjectToLua.ToLuaValue(key));
                    //}
                    table.SetNameValue(this.NameList[0], ObjectToLua.ToLuaValue(key));
                    table.SetNameValue(this.NameList[1], ObjectToLua.ToLuaValue(dict[key]));
                    
                    returnValue = this.Body.Execute(out isBreak);
                    if (returnValue != null || isBreak == true)
                    {
                        isBreak = false;
                        return returnValue;
                    }
                }
            }
            else if (ie != null)
            {
                foreach (object obj in ie)
                {
                    for (int i = 0; i < this.NameList.Count; i++)
                    {
                        table.SetNameValue(this.NameList[i], ObjectToLua.ToLuaValue(obj));
                    }
                    
                    returnValue = this.Body.Execute(out isBreak);
                    if (returnValue != null || isBreak == true)
                    {
                        isBreak = false;
                        return returnValue;
                    }
                }
            }
            else
            {
                // its some other value...
                for (int i = 0; i < this.NameList.Count; i++)
                {
                    table.SetNameValue(this.NameList[i], ObjectToLua.ToLuaValue(state.Value));
                }
                
                returnValue = this.Body.Execute(out isBreak);
                if (returnValue != null || isBreak == true)
                {
                    isBreak = false;
                    return returnValue;
                }
                
                isBreak = false;
                return null;
            }
            isBreak = false;
            return null;
        }
    }
    
}