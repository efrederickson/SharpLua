/*
 * User: elijah
 * Date: 10/23/2012
 * Time: 9:29 PM
 * Copyright 2012 LoDC
 */
using System;
using System.Collections.Generic;
using System.Threading;
using ICSharpCode.SharpDevelop.Dom;

namespace SharpLuaAddIn
{
    /// <summary>
    /// A List of common modules used in "require"
    /// </summary>
    public class Common
    {
        static List<string> asms = new List<string>();
        
        private Common() { }
        
        static Common()
        {
            Thread t = new Thread(new ThreadStart(delegate {
                                                      foreach (DomAssemblyName s in GacInterop.GetAssemblyList())
                                                          asms.Add(s.ShortName);
                                                  }));
            t.Start();
        }
        
        public static string[] ListCLRAssemblies()
        {
            List<string> ret = new List<string>()
            {
                "mscorlib",
                "System",
                "System.Core",
                "System.Windows.Forms",
                "System.Drawing",
                "System.Xml",
                "System.Data",
                "Microsoft.CSharp",
                "Microsoft.VisualBasic",
                "SharpLua",
                "SharpLua.LASM",
                "IExtendFramework",
            };
            
            ret.AddRange(asms);
            
            return ret.ToArray();
        }
        
        public static string[] ListModules()
        {
            /* TODO: Description, extendable (using an xml file?),
                     Load modules from local module path
             */
            return new string[]
            {
                "alien",
                "json",
                "leg",
                "lpeg",
                "cosmo",
                "logging",
                "loop",
                "luadoc",
                "luaidl",
                "luarocks",
                "lxp",
                "lxsh",
                "metalua",
                "oil",
                "pl",
                "socket",
                "vstruct",
                "alt_getopt",
                "get_opt",
                "getopt",
                "base",
                "classlib",
                "cliargs",
                "CLRForm",
                "CLRPackage",
                "copas",
                "coxpcall",
                "date",
                "date2",
                "debug_ext",
                "debug_init",
                "fstable",
                "idl2lua",
                "ilua",
                "io_ext",
                "lanes",
                "lar",
                "lcs",
                "lemock",
                "list",
                "luaunit",
                "LuaXml",
                "lunit",
                "markdown",
                "math_ext",
                "mbox",
                "md5",
                "mime",
                "object",
                "oil",
                "package_ext",
                "parser",
                "posix",
                "lposix",
                "posix_ext",
                "precompiler",
                "preloader",
                "re",
                "rex",
                "serialize",
                "set",
                "stable",
                "std",
                "strict",
                "string_ext",
                "table_ext",
                "tar",
                "unclasslib",
                "tree",
                "xml",
                "luasql",
                "md5",
                "base64",
                "bit",
                "cd",
                "cdcairo",
                "cdcontextplus",
                "cdgdk",
                "cdgl",
                "cdlua51",
                "cdluacairo51",
                "cdluacontextplus51",
                "cdluagl51",
                "cdluaim51",
                "cdluapdf51",
                "cdpdf",
                "des56",
                "ex",
                "freetype6",
                "typical",
                "ftgl",
                "gd",
                "gzio",
                "im",
                "im_avi",
                "im_capture",
                "im_fftw",
                "im_jp2",
                "im_process",
                "im_wmv",
                "imlua_avi51",
                "imlua_capture51",
                "imlua_fftw51",
                "imlua_jp251",
                "imlua_process51",
                "imlua_wmv51",
                "imlua51",
                "iup",
                "iup_pplot",
                "jpeg62",
                "lfs",
                "libcurl",
                "libexpat",
                "libgcrypt",
                "libgd2",
                "libgnutils",
                "libgpg_error",
                "liibiconv2",
                "libintl-8",
                "libmysql",
                "libpng13",
                "libz",
                "lpeg_0_10_2-lpeg",
                "lua51-lanes",
                "luacom",
                "luacurl",
                "luagl",
                "luaglu",
                "luanet",
                "luars232",
                "LuaXML_lib",
                "pack",
                "pcre",
                "pdflib",
                "profiler",
                "rex_pcre",
                "rex_spencer",
                "rings",
                "rxspencer",
                "task",
                "w32",
                "wx",
                "wxmsw28_gl_vc",
                "wxmsw28_stc_vc",
                "wxmsw28_vc",
                "xpm4",
                "zip",
                "zlib1",
                "LuLu",
                "yeuliang",
                "luaparse",
                "lua-inspect",
            };
        }
    }
}
