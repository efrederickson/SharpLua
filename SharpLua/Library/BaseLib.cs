using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpLua.Library
{
    public class BaseLib
    {
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("print", print);
            module.Register("type", type);
            module.Register("getmetatable", getmetatable);
            module.Register("setmetatable", setmetatable);
            module.Register("tostring", tostring);
            module.Register("tonumber", tonumber);
            module.Register("ipairs", ipairs);
            module.Register("pairs", pairs);
            module.Register("next", next);
            module.Register("assert", assert);
            module.Register("error", error);
            module.Register("rawget", rawget);
            module.Register("rawset", rawset);
            module.Register("select", select);
            module.Register("dofile", dofile);
            module.Register("loadstring", loadstring);
            module.Register("unpack", unpack);
            module.Register("pcall", pcall);
            module.Register("openfile", OpenFile);
        }

        public static LuaValue print(LuaValue[] values)
        {
            Console.WriteLine(string.Join<LuaValue>("    ", values));
            return null;
        }

        public static LuaValue type(LuaValue[] values)
        {
            if (values.Length > 0)
            {
                return new LuaString(values[0].GetTypeCode());
            }
            else
            {
                throw new Exception("bad argument #1 to 'type' (value expected)");
            }
        }

        public static LuaValue tostring(LuaValue[] values)
        {
            return new LuaString(values[0].ToString());
        }

        public static LuaValue tonumber(LuaValue[] values)
        {
            LuaString text = values[0] as LuaString;
            if (text != null)
            {
                return new LuaNumber(double.Parse(text.Text));
            }

            LuaString number = values[0] as LuaString;
            if (number != null)
            {
                return number;
            }

            return LuaNil.Nil;
        }

        public static LuaValue setmetatable(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaTable metatable = values[1] as LuaTable;
            table.MetaTable = metatable;
            return null;
        }

        public static LuaValue getmetatable(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            return table.MetaTable;
        }

        public static LuaValue rawget(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaValue index = values[1];
            return table.RawGetValue(index);
        }

        public static LuaValue rawset(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaValue index = values[1];
            LuaValue value = values[2];
            table.SetKeyValue(index, value);
            return null;
        }

        public static LuaValue ipairs(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaFunction func = new LuaFunction(
                (LuaValue[] args) =>
                {
                    LuaTable tbl = args[0] as LuaTable;
                    int index = (int)(args[1] as LuaNumber).Number;
                    int nextIndex = index + 1;

                    if (nextIndex <= tbl.Length)
                    {
                        return new LuaMultiValue(new LuaValue[] { new LuaNumber(nextIndex), tbl.GetValue(nextIndex) });
                    }
                    else
                    {
                        return LuaNil.Nil;
                    }
                }
           );

            return new LuaMultiValue(new LuaValue[] { func, table, new LuaNumber(0) });
        }

        public static LuaValue pairs(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaFunction func = new LuaFunction(next);
            return new LuaMultiValue(new LuaValue[] { func, table, LuaNil.Nil });
        }

        public static LuaValue next(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaValue index = values.Length > 1 ? values[1] : LuaNil.Nil;

            LuaValue prevKey = LuaNil.Nil;
            LuaValue nextIndex = LuaNil.Nil;
            foreach (var key in table.Keys)
            {
                if (prevKey.Equals(index))
                {
                    nextIndex = key;
                    break;
                }
                prevKey = key;
            }

            return new LuaMultiValue(new LuaValue[] { nextIndex, table.GetValue(nextIndex) });
        }

        public static LuaValue assert(LuaValue[] values)
        {
            bool condition = values[0].GetBooleanValue();
            LuaString message = values.Length > 1 ? values[1] as LuaString : null;
            if (message != null)
            {
                throw new LuaError(message.Text);
            }
            else
            {
                throw new LuaError("assertion failed!");
            }
            // return new LuaMultiValue { Values = values };
        }

        public static LuaValue error(LuaValue[] values)
        {
            LuaString message = values[0] as LuaString;
            if (message != null)
            {
                throw new LuaError(message.Text);
            }
            else
            {
                throw new LuaError("error raised!");
            }
        }

        public static LuaValue select(LuaValue[] values)
        {
            LuaNumber number = values[0] as LuaNumber;
            if (number != null)
            {
                int index = (int)number.Number;
                LuaValue[] args = new LuaValue[values.Length - index];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = values[index + i];
                }
                return new LuaMultiValue(args);
            }

            LuaString text = values[0] as LuaString;
            if (text.Text == "#")
            {
                return new LuaNumber(values.Length - 1);
            }

            return LuaNil.Nil;
        }

        public static LuaValue dofile(LuaValue[] values)
        {
            LuaString file = values[0] as LuaString;
            LuaTable enviroment = values[1] as LuaTable;
            return LuaInterpreter.RunFile(file.Text, enviroment);
        }

        public static LuaValue loadstring(LuaValue[] values)
        {
            LuaString code = values[0] as LuaString;
            LuaTable enviroment = values[1] as LuaTable;
            Chunk chunk = LuaInterpreter.Parse(code.Text);

            LuaFunction func = new LuaFunction(
            (LuaValue[] args) =>
            {
                chunk.Enviroment = enviroment;
                return chunk.Execute();
            }
            );

            return func;
        }

        public static LuaValue unpack(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaNumber startNumber = values.Length > 1 ? values[1] as LuaNumber : null;
            LuaNumber lengthNumber = values.Length > 2 ? values[2] as LuaNumber : null;

            int start = startNumber == null ? 1 : (int)startNumber.Number;
            int length = lengthNumber == null ? values.Length : (int)lengthNumber.Number;

            LuaValue[] section = new LuaValue[length];
            for (int i = 0; i < length; i++)
            {
                section[i] = table.GetValue(start + i);
            }
            return new LuaMultiValue(section);
        }

        public static LuaValue pcall(LuaValue[] values)
        {
            LuaFunction func = values[0] as LuaFunction;
            try
            {
                LuaValue[] args = new LuaValue[values.Length - 1];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = values[i + 1];
                }
                LuaValue result = func.Invoke(args);
                return new LuaMultiValue(LuaMultiValue.UnWrapLuaValues(new LuaValue[] { LuaBoolean.True, result }));
            }
            catch (Exception error)
            {
                return new LuaMultiValue(new LuaValue[] { LuaBoolean.False, new LuaString(error.Message) });
            }
        }
        
        public static LuaValue OpenFile(LuaValue[] values)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Lua files|*.lua|SharpLua files|*.slua|wLua files|*.wlua|MetaLua files|*.mlua|All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Loading file '" + Path.GetFileName(ofd.FileName) + "'...");
                return LuaInterpreter.RunFile(ofd.FileName, Lua.GlobalEnvironment);
            }
            else
                return LuaNil.Nil;
        }
    }
}
