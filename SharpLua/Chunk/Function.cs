using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A Lua function
    /// </summary>
    [Serializable()]
    public partial class Function : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">The environment to run in</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaTable table = enviroment;

            if (this.Name.MethodName == null)
            {
                for (int i = 0; i < this.Name.FullName.Count - 1; i++)
                {
                    LuaValue obj = enviroment.GetValue(this.Name.FullName[i]);
                    table = obj as LuaTable;

                    if (table == null)
                    {
                        throw new Exception("Not a table: " + this.Name.FullName[i]);
                    }
                }

                table.SetNameValue(
                    this.Name.FullName[this.Name.FullName.Count - 1],
                    this.Body.Evaluate(enviroment));
            }
            else
            {
                for (int i = 0; i < this.Name.FullName.Count; i++)
                {
                    LuaValue obj = enviroment.GetValue(this.Name.FullName[i]);
                    
                    if ((obj as LuaClass) != null)
                        table = (obj as LuaClass).Self;
                    else
                        table = obj as LuaTable;
                    
                    if (table == null)
                    {
                        throw new Exception("Not a table: " + this.Name.FullName[i]);
                    }
                }

                this.Body.ParamList.NameList.Insert(0, "self");

                table.SetNameValue(
                    this.Name.MethodName,
                    this.Body.Evaluate(enviroment));
            }

            isBreak = false;
            return null;
        }
    }
}
