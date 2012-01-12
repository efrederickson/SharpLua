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
        /// <summary>
        /// Added by Arjen...initialize .NET object as LuaUserData using reflection
        /// </summary>
        /// <param name="obj">.NET object</param>
        /// <param name="init">True if object should reflect in LUA else empty metatable</param>
        public LuaUserdata(object obj, bool init)
        {
            if (init)
            {
                MetaTable = SharpLua.ObjectToLua.GetControlMetaTable();
                this.Object = obj;
            }
            else
            {
                MetaTable = new LuaTable();
                this.Object = obj;
            }
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
