using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    /// <summary>
    /// Defines a module for use in SharpLua. If the module name is not specified,
    /// it defaults to the class name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LuaModuleAttribute : Attribute
    {
        public string ModuleName { get; internal set; }

        public LuaModuleAttribute(string name)
        {
            ModuleName = name;
        }

        public LuaModuleAttribute()
        {
            ModuleName = "";
        }
    }
}
