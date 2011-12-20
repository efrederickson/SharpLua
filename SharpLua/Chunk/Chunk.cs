using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public partial class Chunk
    {
        public LuaTable Enviroment;

        public LuaValue Execute()
        {
            bool isBreak;
            return this.Execute(out isBreak);
        }

        public LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            this.Enviroment = new LuaTable(enviroment);
            return this.Execute(out isBreak);
        }

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
