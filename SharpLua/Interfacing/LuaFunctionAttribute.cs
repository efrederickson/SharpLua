using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua
{
    /// <summary>
    /// Defines a function for use in a SharpLua module that has the 
    /// ModuleAttribute applied to the class. If the name is not specified,
    /// it defaults to the method name
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaFunctionAttribute : Attribute
    {
        public string FunctionName { get; internal set; }

        public LuaFunctionAttribute(string name)
        {
            FunctionName = name;
        }

        public LuaFunctionAttribute()
        {
            FunctionName = "";
        }
    }
}
