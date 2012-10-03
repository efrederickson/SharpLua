/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/27/2011
 * Time: 10:54 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// FileSystem access class (similar to lfs)
    /// </summary>
    public class FileSystemLib
    {
        internal static string currentDir = null;
        
        public static void RegisterModule(LuaTable env)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            env.SetNameValue("filesystem", module); // TODO: better name
        }
        
        public static void RegisterFunctions(LuaTable mod)
        {
            mod.Register("attributes", Attributes);
            mod.Register("chdir", ChDir);
            mod.Register("lock", Lock);
            mod.Register("currentdir", CurrentDir);
            mod.Register("mkdir", Mkdir);
            mod.Register("delete", Delete);
            mod.Register("unlock", Unlock);
            mod.Register("copy", (LuaValue[] args) =>
                         {
                             File.Copy(args[0].ToString(), args[1].ToString());
                             return LuaBoolean.True;
                         });
            mod.Register("write", (LuaValue[] args) =>
                         {
                             using (StreamWriter sw = new StreamWriter(args[0].ToString()))
                             {
                                 for (int i = 1; i < args.Length; i++)
                                     sw.WriteLine(args[i].ToString());
                                 sw.Close();
                             }
                             return LuaNil.Nil;
                         });
        }
        
        public static LuaValue Attributes(LuaValue[] args)
        {
            string fn = args[0].ToString();
            LuaTable ret = new LuaTable();
            FileInfo f = new FileInfo(fn);
            
            ret.SetNameValue("filename", new LuaString(fn));
            ret.SetNameValue("dir", new LuaString(f.DirectoryName));
            ret.SetNameValue("drive", new LuaString(Path.GetPathRoot(f.DirectoryName)));
            ret.SetNameValue("attributes", new LuaString(f.Attributes.ToString()));
            ret.SetNameValue("access", new LuaString(f.LastAccessTime.ToString()));
            ret.SetNameValue("modification", new LuaString(f.LastWriteTime.ToString()));
            ret.SetNameValue("ext", new LuaString(f.Extension));
            ret.SetNameValue("size", new LuaString(f.Length.ToString()));
            
            return ret;
        }
        
        public static LuaValue ChDir(LuaValue[] args)
        {
            string path = args[0].ToString();
            if (Directory.Exists(path))
            {
                currentDir = path;
                return LuaBoolean.True;
            }
            else
            {
                return new LuaMultiValue(new LuaValue[] { LuaBoolean.False, new LuaString("Directory does not exist!") });
            }
        }
        
        public static LuaValue Lock(LuaValue[] args)
        {
            string path = args[0].ToString();
            if (!Directory.Exists(path))
                return new LuaMultiValue(new LuaValue[] { LuaBoolean.False, new LuaString("Directory does not exist!") });
            
            using (StreamWriter sw = new StreamWriter(path + "\\sharplua.lock"))
            {
                sw.WriteLine("[PASSWORD]=[NULL (IMPLEMENT)]");
                sw.WriteLine("[DATEOFCREATION]=" + DateTime.Now.ToString());
                // add one week to the date
                sw.WriteLine("[EXPIRETIME]=" + DateTime.Now.AddDays(7).ToString());
                sw.Close();
            }
            FileInfo fi = new FileInfo(path + "\\sharplua.lock");
            fi.Attributes = FileAttributes.Hidden | 
                FileAttributes.System | 
                FileAttributes.NotContentIndexed;
            
            return LuaBoolean.True;
        }
        
        public static LuaValue CurrentDir(LuaValue[] args)
        {
            return new LuaString(currentDir);
        }
        
        public static LuaValue Mkdir(LuaValue[] args)
        {
            try {
                System.IO.Directory.CreateDirectory(args[0].ToString());
            } catch (Exception ex) {
                return new LuaMultiValue(new LuaValue[] {LuaBoolean.False, new LuaString(ex.Message) });
            }
            return LuaBoolean.True;
        }
        
        public static LuaValue Delete(LuaValue[] args)
        {
            string path = args[0].ToString();
            if (File.Exists(path))
                File.Delete(path);
            else
                Directory.Delete(path);
            return LuaBoolean.True;
        }
        
        public static LuaValue Unlock(LuaValue[] args)
        {
            string path = args[0].ToString();
            if (!Directory.Exists(path))
                return new LuaMultiValue(new LuaValue[] { LuaBoolean.False, new LuaString("Directory does not exist!") });
            
            File.Delete(path + "\\sharplua.lock");
            return LuaBoolean.True;
        }
    }
}