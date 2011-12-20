using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class LuaNumber : LuaValue
    {
        public LuaNumber(double number)
        {
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
            return this.Number.ToString();
        }
    }
}
