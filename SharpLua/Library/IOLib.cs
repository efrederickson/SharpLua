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
            module.Register("lines", Lines);
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
        private static LuaValue GetLines(LuaValue[] args)
        {
            List<LuaString> l = new List<LuaString>();
            TextReader tr = null;
            if (args.Length > 0)
                tr = new StreamReader((args[0] as LuaString).Text);
            else
                tr = Input(null).Value as TextReader;

            string line = tr.ReadLine();
            while (line != null)
            {
                l.Add(new LuaString(line));
                line = tr.ReadLine();
            }
            tr.Close();
            return new LuaMultiValue(l.ToArray());
        }

        private static LuaValue NextLine(LuaValue[] values)
        {
            LuaMultiValue table = values[0] as LuaMultiValue;
            LuaValue loopVar = values[1];
            LuaValue result = LuaNil.Nil;

            int idx = (loopVar == LuaNil.Nil ? 0 : Convert.ToInt32(loopVar.MetaTable.GetValue("__lines_index").Value));

            if (idx < table.Values.Length)
            {
                result = table.Values[idx];
                result.MetaTable.SetNameValue("__lines_index", new LuaNumber(idx + 1));
            }
            return result;
        }

        public static LuaValue Lines(LuaValue[] args)
        {
            LuaFunction f = new LuaFunction(NextLine);
            return new LuaMultiValue(new LuaValue[] { f, GetLines(args), LuaNil.Nil });
        }

    }
}