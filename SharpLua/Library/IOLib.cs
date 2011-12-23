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
            module.Register("input", Input);
            module.Register("output", Output);
            module.Register("open", Open);
            module.Register("read", Read);
            module.Register("write", Write);
            module.Register("flush", Flush);
            module.Register("tmpfile", TmpFile);
            module.Register("close", Close);
        }

        private static TextReader DefaultInput = Console.In;
        private static TextWriter DefaultOutput = Console.Out;

        public static LuaValue Input(LuaValue[] values)
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

        public static LuaValue Output(LuaValue[] values)
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

        public static LuaValue Open(LuaValue[] values)
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

        public static LuaValue Read(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(Input(null));
            args.AddRange(values);
            return FileLib.Read(args.ToArray());
        }

        public static LuaValue Write(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(Output(null));
            args.AddRange(values);
            return FileLib.Write(args.ToArray());
        }

        public static LuaValue Flush(LuaValue[] values)
        {
            return FileLib.Flush(new LuaValue[] { Output(null) });
        }

        public static LuaValue TmpFile(LuaValue[] values)
        {
            StreamWriter writer = File.CreateText(Path.GetTempFileName());
            return new LuaUserdata(writer);
        }
        
        public static LuaValue Close(LuaValue[] args)
        {
            LuaUserdata data = args[0] as LuaUserdata;
            TextReader reader = data.Value as TextReader;
            if (reader != null)
            {
                reader.Close();
                return null;
            }

            TextWriter writer = data.Value as TextWriter;
            if (writer != null)
            {
                writer.Close();
            }

            return null;
        }
    }
}
