using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class ForStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaNumber start = this.Start.Evaluate(enviroment) as LuaNumber;
            LuaNumber end = this.End.Evaluate(enviroment) as LuaNumber;

            double step = 1;
            if (this.Step != null)
            {
                step = (this.Step.Evaluate(enviroment) as LuaNumber).Number;
            }

            var table = new LuaTable(enviroment);
            table.SetNameValue(this.VarName, start);
            this.Body.Enviroment = table;

            while (step > 0 && start.Number <= end.Number ||
                   step <= 0 && start.Number >= end.Number)
            {
                var returnValue = this.Body.Execute(out isBreak);
                if (returnValue != null || isBreak == true)
                {
                    isBreak = false;
                    return returnValue;
                }
                start.Number += step;
            }

            isBreak = false;
            return null;
        }
    }
}
