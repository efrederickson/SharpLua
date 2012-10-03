using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    [Serializable()]
    public class LuaNil : LuaValue
    {
        public static readonly LuaNil Nil = new LuaNil();

        private LuaNil() { MetaTable = new LuaTable(); }

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
            if (this.MetaTable != null)
            {
                LuaFunction function = this.MetaTable.GetValue("__tostring") as LuaFunction;
                if (function != null)
                {
                    return function.Invoke(new LuaValue[] { this }).ToString();
                }
            }
            
            return "nil";
        }
    }
}
