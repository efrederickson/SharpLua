using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    [Serializable()]
    public abstract class LuaValue : IEquatable<LuaValue>
    {
        
        public abstract object Value { get; }
        
        public abstract string GetTypeCode();
        
        public virtual bool GetBooleanValue()
        {
            return true;
        }
        
        public bool Equals(LuaValue other)
        {
            if (other == null)
            {
                return false;
            }

            if (this is LuaNil)
            {
                return other is LuaNil;
            }

            if (this is LuaTable && other is LuaTable)
            {
                return object.ReferenceEquals(this, other);
            }

            return this.Value.Equals(other.Value);
        }
        
        public override int GetHashCode()
        {
            if (this is LuaNumber || this is LuaString)
            {
                return this.Value.GetHashCode();
            }

            return base.GetHashCode();
        }
        
        public static LuaValue GetKeyValue(LuaValue baseValue, LuaValue key)
        {
            LuaTable table = baseValue as LuaTable;
            if (((baseValue as LuaClass) != null) && table == null)
                table = (baseValue as LuaClass).Self;
            
            if (table != null)
            {
                return table.GetValue(key);
            }
            else
            {
                LuaUserdata userdata = baseValue as LuaUserdata;
                if (userdata != null)
                {
                    if (userdata.MetaTable != null)
                    {
                        LuaValue index = userdata.MetaTable.GetValue("__index");
                        if (index != null)
                        {
                            LuaFunction func = index as LuaFunction;
                            if (func != null)
                            {
                                // its a getter function
                                return func.Invoke(new LuaValue[] { baseValue, key });
                            }
                            else
                            {
                                // its a probably a table
                                return GetKeyValue(index, key);
                            }
                        }
                    }
                }
                
                throw new Exception(string.Format("Access field '{0}' not from a table.", key.Value));
            }
        }
        
        public LuaTable MetaTable { get; set; }
    }
}
