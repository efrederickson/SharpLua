using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A local variable
    /// </summary>
    [Serializable()]
    public partial class LocalVar : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">The environment to run in</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaValue[] values = this.ExprList.ConvertAll(expr => expr.Evaluate(enviroment)).ToArray();
            LuaValue[] neatValues = LuaMultiValue.UnWrapLuaValues(values);

            for (int i = 0; i < Math.Min(this.NameList.Count, neatValues.Length); i++)
            {
                enviroment.RawSetValue(this.NameList[i], neatValues[i]);
            }

            if (neatValues.Length < this.NameList.Count)
            {
                for (int i = neatValues.Length; i < this.NameList.Count - neatValues.Length; i++)
                {
                    enviroment.RawSetValue(this.NameList[i], LuaNil.Nil);
                }
            }

            isBreak = false;
            return null;
        }
    }
}
