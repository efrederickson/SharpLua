using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class LuaNil : LuaValue
    {
        public static readonly LuaNil Nil = new LuaNil();

        private LuaNil() { }

        public override object Value
        {
            get { return null; }
        }

        public override string GetTypeCode()
        {
            return "nil";
        }

        public override bool GetBooleanValue()
        {
            return false;
        }

        public override string ToString()
        {
            return "nil";
        }
    }
}
