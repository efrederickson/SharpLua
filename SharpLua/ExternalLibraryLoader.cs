/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/21/2011
 * Time: 10:52 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;

namespace SharpLua
{
    /// <summary>
    /// Loads dll's and exe's that have the ILuaLibrary interface
    /// </summary>
    public  class ExternalLibraryLoader
    {
        private ExternalLibraryLoader()
        { }
        
        /// <summary>
        /// Loads a dll or exe and checks for Lua libraries
        /// </summary>
        /// <param name="FileName"></param>
        public static void Load(string FileName)
        {
            try {
                //Create a new assembly from the plugin file we're adding..
                Assembly pluginAssembly = Assembly.LoadFrom(FileName);
                //Next we'll loop through all the Types found in the assembly
                if (pluginAssembly != null) {
                    int itemcount = 0;
                    int pluginsfound = 0;
                    foreach (Type pluginType in pluginAssembly.GetTypes()) {
                        //Only look at public types
                        if (pluginType.IsPublic) {
                            //Only look at non-abstract types
                            if (!pluginType.IsAbstract) {
                                //Log.WriteLine(String.Format("PluginManager: Checking Type '{0}' from {1}", pluginType.Name, FileName))
                                itemcount += 1;
                                //Gets a type object of the interface we need the plugins to match
                                Type typeInterface = pluginType.GetInterface("SharpLua.Library.ILuaLibrary", true);

                                //Make sure the interface we want to use actually exists
                                if ((typeInterface != null)) {
                                    try {
                                        Library.ILuaLibrary lib= (Library.ILuaLibrary)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
                                        //Call initialization for the plugin
                                        lib.RegisterModule(Lua.GlobalEnvironment);

                                    } catch (Exception ex) {
                                        Console.WriteLine("Error: " + ex.ToString());
                                    }
                                } else {
                                }
                                typeInterface = null;
                                // Clean up
                            }
                        }
                    }
                }
                if (pluginAssembly == null) {
                    throw new Exception("Empty Assembly!");
                }
                pluginAssembly = null;
                //more cleanup
            } catch (Exception e) {
            }
        }
    }
}
