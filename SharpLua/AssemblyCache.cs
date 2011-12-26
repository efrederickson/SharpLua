using System;
using System.Collections;
using System.Reflection;
using System.IO;

namespace SharpLua
{
    /// <summary>
    /// Keeps a list of the assemblies that are loaded. Ensures that we don't
    /// load an assembly more than once.
    /// </summary>
    public class AssemblyCache
    {
        private static AssemblyCache instance;

        private const int CAPACITY = 5000;
        private Hashtable assemblyTable = new Hashtable(CAPACITY);

        /// <summary>
        /// Private constructor ensures singleton design pattern
        /// </summary>
        private AssemblyCache()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Add(assembly);
            }
        }

        /// <summary>
        /// Returns the singleton instance (creating it if necessary).
        /// </summary>
        /// <returns></returns>
        public static AssemblyCache Instance
        {
            get
            {
                if (instance == null)
                    instance = new AssemblyCache();
                return instance;
            }
        }

        /// <summary>
        /// Loads an assembly, either from the GAC or from a file
        /// </summary>
        /// <param name="assembly">An assembly name or assembly file name</param>
        /// <returns>An Assembly object</returns>
        public Assembly LoadAssembly (string assembly)
        {
            assembly = assembly.ToLower(); // we dont want something like
            // SYSTEM.WINDOWS.FORMS and
            // System.Windows.Forms, so 1 entry
            // per filename
            
            Assembly o = (Assembly) assemblyTable[assembly];
            if (o != null) // the assembly is already there.
                return (Assembly) o;
            
            if (System.IO.Path.IsPathRooted(assembly))
                o = Assembly.LoadFrom(assembly);
            else
            {
                // TODO: change this to Assembly.Load
                // o = Assembly.Load(assembly);
                o = Assembly.LoadWithPartialName(assembly);
                
                // If the loaded assembly is null, its probably
                // in the form of "filename.dll"
                if (o == null)
                    o = Assembly.LoadFrom(assembly);
            }
            if (o == null)
            { // Getting desperate here.
                o = Assembly.Load(assembly);
            }
            assemblyTable[assembly] = o; // may be an empty entry,
            // but if its not defined, attempting retrieval
            // will cause an error.
            return (Assembly) o;
        }

        /// <summary>
        /// Adds a new assembly to the assembly cache
        /// </summary>
        /// <param name="assembly"></param>
        public void Add(Assembly assembly)
        {
            assemblyTable[assembly.FullName] = assembly;
        }

        /// <summary>
        /// Removes an assembly from the assembly cache
        /// </summary>
        /// <param name="assembly"></param>
        public void Remove(Assembly assembly)
        {
            assemblyTable.Remove(assembly.FullName);
        }

        /// <summary>
        /// Returns an array of all loaded assemblies
        /// </summary>
        /// <returns></returns>
        public Assembly[] Assemblies
        {
            get
            {
                return (Assembly[]) new ArrayList(assemblyTable.Values).ToArray(typeof(Assembly));
            }
        }
        
        private static Hashtable typeTable = new Hashtable(CAPACITY);
        private static Hashtable usingTable = new Hashtable(CAPACITY);

        /// <summary>
        /// Clears the cache and resets its state to default values
        /// </summary>
        public static void Clear()
        {
            typeTable = new Hashtable(CAPACITY);
            usingTable = new Hashtable(CAPACITY);
            ImportNamespace("System");
            ImportNamespace("LSharp");
        }

        /// <summary>
        /// Finds a type given its fully qualified or unqualified name
        /// Takes advantage of the cache to speed up lookups
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type FindType (string type)
        {
            object o = typeTable[type.ToLower()];

            if (o == null)
            {
                o = SearchType(type);
                typeTable[type.ToLower()] = o;
            }

            return (Type) o;
        }

        /// <summary>
        /// adds a namespace to the Type Cache (similar to using in C#)
        /// </summary>
        /// <param name="name"></param>
        public static void ImportNamespace (string name)
        {
            usingTable[name] = name;
        }

        private static Type SearchType (string typeName)
        {
            // I wonder whether there is a better way to do this, maybe using Assembly.GetAssembly() ?
            // needs further investigation

            // Look up the type in the current assembly
            Type type = Type.GetType(typeName, false, true);
            if (type != null)
                return type;

            // Look up the type in all loaded assemblies
            foreach(Assembly assembly in  AssemblyCache.Instance.Assemblies)
            {
                type = assembly.GetType(typeName,false, true);
                if (type != null)
                    return type;
            }

            // Try to use the using directives to guess the namespace ..
            foreach (string name in usingTable.Values)
            {
                foreach(Assembly assembly in  AssemblyCache.Instance.Assemblies)
                {
                    type = assembly.GetType(string.Format("{0}.{1}",name, typeName),false, true);
                    if (type != null)
                        return type;

                }

                type = Type.GetType(string.Format("{0}.{1}",name, typeName),false, true);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
