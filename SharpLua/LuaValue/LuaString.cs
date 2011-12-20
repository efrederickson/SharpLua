using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class LuaString : LuaValue
    {
        public LuaString(string text)
        {
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
            return this.Text;
        }
    }
}
