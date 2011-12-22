/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/21/2011
 * Time: 10:53 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SharpLua.Library
{
    /// <summary>
    /// A Lua Library extension. ALL FUNCTIONS **MUST** BE STATIC.
    /// </summary>
    public interface ILuaLibrary
    {
        void RegisterModule(LuaTable environment);
    }
}
