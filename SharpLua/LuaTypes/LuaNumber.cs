using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    [Serializable()]
    public class LuaNumber : LuaValue
    {
        public LuaNumber(double number)
        {
            MetaTable = new LuaTable();
            this.Number = number;
        }

        public double Number { get; set; }

        public override object Value
        {
            get { return this.Number; }
        }

        public override string GetTypeCode()
        {
            return "number";
        }

        public override string ToString()
        {
            if (this.MetaTable != null)
            {
                LuaFunction function = this.MetaTable.GetValue("__tostring") as LuaFunction;
                if (function != null)
                {
                    return function.Invoke(new LuaValue[] { this }).ToString();
                }
            }
            
            return this.Number.ToString();
        }
    }
}
