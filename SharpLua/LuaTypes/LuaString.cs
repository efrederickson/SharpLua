using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    public class LuaString : LuaValue
    {
        public LuaString(string text)
        {
            MetaTable = new LuaTable();
            this.Text = text;
        }

        public static readonly LuaString Empty = new LuaString(string.Empty);

        public string Text { get; set; }

        public override object Value
        {
            get { return this.Text; }
        }

        public override string GetTypeCode()
        {
            return "string";
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
            
            return this.Text;
        }
    }
}
