using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// Base class for lua code
    /// </summary>
    public partial class Chunk
    {
        /// <summary>
        /// The environment to run in
        /// </summary>
        public LuaTable Enviroment;
        
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <returns></returns>
        public LuaValue Execute()
        {
            bool isBreak;
            return this.Execute(out isBreak);
        }
        
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">Runs in the given environment</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            this.Enviroment = new LuaTable(enviroment);
            return this.Execute(out isBreak);
        }
        
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public LuaValue Execute(out bool isBreak)
        {
            foreach (Statement statement in Statements)
            {
                ReturnStmt returnStmt = statement as ReturnStmt;
                if (returnStmt != null)
                {
                    isBreak = false;
                    return LuaMultiValue.WrapLuaValues(returnStmt.ExprList.ConvertAll(expr => expr.Evaluate(this.Enviroment)).ToArray());
                }
                else if (statement is BreakStmt)
                {
                    isBreak = true;
                    return null;
                }
                else
                {
                    var returnValue = statement.Execute(this.Enviroment, out isBreak);
                    if (returnValue != null || isBreak == true)
                    {
                        return returnValue;
                    }
                }
            }
            
            isBreak = false;
            return null;
        }
    }
}
