using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public static class IOLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("io", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("input", input);
            module.Register("output", output);
            module.Register("open", open);
            module.Register("read", read);
            module.Register("write", write);
            module.Register("flush", flush);
            module.Register("tmpfile", tmpfile);
        }

        private static TextReader DefaultInput = Console.In;
        private static TextWriter DefaultOutput = Console.Out;

        public static LuaValue input(LuaValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new LuaUserdata(DefaultInput);
            }
            else
            {
                LuaString file = values[0] as LuaString;
                if (file != null)
                {
                    DefaultInput = File.OpenText(file.Text);
                    return null;
                }

                LuaUserdata data = values[0] as LuaUserdata;
                if (data != null && data.Value is TextReader)
                {
                    DefaultInput = data.Value as TextReader;
                }
                return null;
            }
        }

        public static LuaValue output(LuaValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new LuaUserdata(DefaultOutput);
            }
            else
            {
                LuaString file = values[0] as LuaString;
                if (file != null)
                {
                    DefaultOutput = File.CreateText(file.Text);
                    return null;
                }

                LuaUserdata data = values[0] as LuaUserdata;
                if (data != null && data.Value is TextWriter)
                {
                    DefaultOutput = data.Value as TextWriter;
                }
                return null;
            }
        }

        public static LuaValue open(LuaValue[] values)
        {
            LuaString file = values[0] as LuaString;
            LuaString modeStr = values.Length > 1 ? values[1] as LuaString : null;
            string mode = modeStr == null ? "r" : modeStr.Text;

            switch (mode)
            {
                case "r":
                case "r+":
                    StreamReader reader = File.OpenText(file.Text);
                    return new LuaUserdata(reader, FileLib.CreateMetaTable());
                case "w":
                case "w+":
                    StreamWriter writer = File.CreateText(file.Text);
                    return new LuaUserdata(writer, FileLib.CreateMetaTable());
                case "a":
                case "a+":
                    writer = File.AppendText(file.Text);
                    return new LuaUserdata(writer, FileLib.CreateMetaTable());
                default:
                    throw new ArgumentException("Invalid file open mode " + mode);
            }
        }

        public static LuaValue read(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(input(null));
            args.AddRange(values);
            return FileLib.read(args.ToArray());
        }

        public static LuaValue write(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(output(null));
            args.AddRange(values);
            return FileLib.write(args.ToArray());
        }

        public static LuaValue flush(LuaValue[] values)
        {
            return FileLib.flush(new LuaValue[] { output(null) });
        }

        public static LuaValue tmpfile(LuaValue[] values)
        {
            StreamWriter writer = File.CreateText(Path.GetTempFileName());
            return new LuaUserdata(writer);
        }
    }
}
