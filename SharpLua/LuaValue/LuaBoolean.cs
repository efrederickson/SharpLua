using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class LuaBoolean : LuaValue
    {
        public static readonly LuaBoolean False = new LuaBoolean { BoolValue = false };

        public static readonly LuaBoolean True = new LuaBoolean { BoolValue = true };

        public bool BoolValue { get; set; }

        public override object Value
        {
            get { return this.BoolValue; }
        }

        public override string GetTypeCode()
        {
            return "boolean";
        }

        public override bool GetBooleanValue()
        {
            return this.BoolValue;
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
            
            return this.BoolValue.ToString().ToLower();
        }

        private LuaBoolean() { MetaTable = new LuaTable(); }

        public static LuaBoolean From(bool value)
        {
            if (value == true)
            {
                return True;
            }
            else
            {
                return False;
            }
        }
    }
}
