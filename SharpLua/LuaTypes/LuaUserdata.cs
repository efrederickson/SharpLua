using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    [Serializable()]
    public class LuaUserdata : LuaValue
    {
        private object Object;

        public LuaUserdata(object obj)
        {
            MetaTable = new LuaTable();
            this.Object = obj;
        }

        public LuaUserdata(object obj, LuaTable metatable)
        {
            this.Object = obj;
            this.MetaTable = metatable;
        }

        public override object Value
        {
            get { return this.Object; }
        }
        
        public override string GetTypeCode()
        {
            return "userdata";
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
            
            return "userdata";
        }
    }
}
