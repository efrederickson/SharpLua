using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    public class LuaUserdata : LuaValue
    {
        private object Object;

        public LuaUserdata(object obj)
        {
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

        public LuaTable MetaTable { get; set; }

        public override string GetTypeCode()
        {
            return "userdata";
        }

        public override string ToString()
        {
            return "userdata";
        }
    }
}
