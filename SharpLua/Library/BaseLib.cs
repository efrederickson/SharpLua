using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SharpLua.AST;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public class BaseLib
    {
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("print", Print);
            module.Register("type", Type);
            module.Register("getmetatable", GetMetaTable);
            module.Register("setmetatable", SetMetaTable);
            module.Register("tostring", Tostring);
            module.Register("tonumber", Tonumber);
            module.Register("ipairs", IPairs);
            module.Register("pairs", Pairs);
            module.Register("next", Next);
            module.Register("assert", Assert);
            module.Register("error", Error);
            module.Register("rawget", RawGet);
            module.Register("rawset", RawSet);
            module.Register("select", Select);
            module.Register("dofile", DoFile);
            module.Register("loadstring", LoadString);
            module.Register("unpack", UnPack);
            module.Register("pcall", Pcall);
            module.Register("openfile", OpenFile);
            module.Register("require", Require);
            module.Register("set", Set);
            module.Register("loadfile", LoadFile);
            module.Register("xpcall", XPcall);
            module.Register("wait", Wait);
            module.Register("loadbin", LoadBin);
            module.Register("saveenv", SaveEnvironment);
            module.Register("loadenv", LoadEnvironment);
        }

        public static LuaValue Print(LuaValue[] values)
        {
            Console.WriteLine(string.Join<LuaValue>("    ", values));
            return null;
        }

        public static LuaValue Type(LuaValue[] values)
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

        public static LuaValue Tostring(LuaValue[] values)
        {
            return new LuaString(values[0].ToString());
        }

        public static LuaValue Tonumber(LuaValue[] values)
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

        public static LuaValue SetMetaTable(LuaValue[] values)
        {
            if ((values[0] as LuaClass) != null)
                throw new Exception("Cannot set meta tables on Classes, use functions instead.");
            LuaValue val = (LuaValue) values[0];
            if (val.MetaTable == null)
                val.MetaTable = new LuaTable();
            LuaTable metatable = (LuaTable) values[1];
            TableLib.Copy(new LuaValue[] { val.MetaTable, metatable });
            return null;
        }

        public static LuaValue GetMetaTable(LuaValue[] values)
        {
            if ((values[0] as LuaClass) != null)
                return ((values[0] as LuaClass).Self).MetaTable;
            return values[0].MetaTable;
        }

        public static LuaValue RawGet(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaValue index = values[1];
            return table.RawGetValue(index);
        }

        public static LuaValue RawSet(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            if ((table == null) && ((values[0] as LuaClass) != null))
                table = (values[0] as LuaClass).Self;
            LuaValue index = values[1];
            LuaValue value = values[2];
            table.RawSetValue(index.Value.ToString(), value);
            return null;
        }

        public static LuaValue IPairs(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaFunction func = new LuaFunction(
                (LuaValue[] args) =>
                {
                    LuaTable tbl = args[0] as LuaTable;
                    int index = (int)(args[1] as LuaNumber).Number;
                    int nextIndex = index + 1;

                    if (nextIndex <= tbl.Count)
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

        public static LuaValue Pairs(LuaValue[] values)
        {
            LuaTable table = values[0] as LuaTable;
            LuaFunction func = new LuaFunction(Next);
            return new LuaMultiValue(new LuaValue[] { func, table, LuaNil.Nil });
        }

        public static LuaValue Next(LuaValue[] values)
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

        public static LuaValue Assert(LuaValue[] values)
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

        public static LuaValue Error(LuaValue[] values)
        {
            if (values.Length > 0)
            {
                LuaString message = values[0] as LuaString;
                throw new LuaError(message.Text);
            }
            else
            {
                throw new LuaError("Error!");
            }
        }

        public static LuaValue Select(LuaValue[] values)
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

        public static LuaValue DoFile(LuaValue[] values)
        {
            LuaString file = values[0] as LuaString;
            LuaTable enviroment = LuaRuntime.GlobalEnvironment;
            return LuaRuntime.RunFile(file.Text, enviroment);
        }

        public static LuaValue LoadString(LuaValue[] values)
        {
            LuaString code = values[0] as LuaString;
            LuaTable enviroment = LuaRuntime.GlobalEnvironment;
            Chunk chunk = LuaRuntime.Parse(code.Text);

            LuaFunction func = new LuaFunction(
                (LuaValue[] args) =>
                {
                    chunk.Enviroment = enviroment;
                    return chunk.Execute();
                }
               );

            return func;
        }

        public static LuaValue UnPack(LuaValue[] values)
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

        public static LuaValue Pcall(LuaValue[] values)
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
            ofd.Filter = "Lua files|*.lua|SharpLua files|*.slua|wLua files|*.wlua|All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Loading file '" + Path.GetFileName(ofd.FileName) + "'...");
                return LuaRuntime.RunFile(ofd.FileName, LuaRuntime.GlobalEnvironment);
            }
            else
                return LuaNil.Nil;
        }
        
        public static LuaValue Require(LuaValue[] args)
        {
            // get loaders table
            LuaTable t = (LuaRuntime.GlobalEnvironment.GetValue("package") as LuaTable).GetValue("loaders") as LuaTable;
            if (t == null)
                throw new Exception("Cannot get loaders table from package module!");
            if (t.Count == 0)
                throw new Exception("Loaders table is empty!");
            // whether package was found/loaded
            LuaBoolean b = LuaBoolean.False;
            LuaTable module = null;
            foreach (LuaValue key in t.Keys)
            {
                LuaFunction f = t.GetValue(key) as LuaFunction;
                if (f != null)
                {
                    LuaMultiValue lmv = f.Invoke(new LuaValue[] {new LuaString(args[0].Value.ToString())}) as LuaMultiValue;
                    b = lmv.Values[0] as LuaBoolean;
                    if (b.BoolValue == true)
                    {
                        module = lmv.Values[1] as LuaTable;
                        break;
                    }
                }
                else
                {
                    throw new Exception("Cannot cast type '" + t.GetValue(key).GetType().ToString() + "' to type 'LuaFunction'");
                }
            }
            if (b.BoolValue == false)
            {
                Console.WriteLine("Could not load package '" + args[0].Value.ToString() + "'!");
            }
            return module;
        }
        
        public static LuaValue Set(LuaValue[] args)
        {
            // set table[key] = value
            LuaTable t = args[0] as LuaTable;
            t.SetKeyValue(args[1], args[2]);
            return t;
        }
        
        public static LuaValue LoadFile(LuaValue[] args)
        {
            return DoFile(args);
        }
        
        public static LuaValue XPcall(LuaValue[] args)
        {
            // pcall with error handler function
            // usage: xpcall(func, errorHandlerFunc)
            // success output: true, <func output>
            // failue output: false, error message, <error function output>
            LuaFunction f = args[0] as LuaFunction;
            LuaFunction ef = args[1] as LuaFunction;
            LuaValue v = null;
            try {
                v =  f.Invoke(new LuaValue[] {});
            } catch (Exception ex) {
                v = ef.Invoke(new LuaValue[] { new LuaUserdata(ex)});
                return new LuaMultiValue(new LuaValue[] { LuaBoolean.False, new LuaString(ex.Message), v});
            }
            return new LuaMultiValue(new LuaValue[] {LuaBoolean.True, v });
        }
        
        public static LuaValue Wait(LuaValue[] args)
        {
            LuaNumber time = args[0] as LuaNumber;
            if (time == null)
                throw new Exception("object '" + args[0] + "' is not a number!");
            System.Threading.Thread.Sleep(int.Parse((time.Number * 1000).ToString()));
            return null;
        }
        
        public static LuaValue LoadBin(LuaValue[] args)
        {
            string fn = (args[0] as LuaString).Text;
            Chunk c = (Chunk) Serializer.Deserialize(fn);
            bool success;
            c.Enviroment = LuaRuntime.GlobalEnvironment;
            return c.Execute(LuaRuntime.GlobalEnvironment, out success);
        }
        
        public static LuaValue SaveEnvironment(LuaValue[] args)
        {
            string fn = (args[0] as LuaString).Text;
            Serializer.Serialize(LuaRuntime.GlobalEnvironment, fn);
            return LuaNil.Nil;
        }
        
        public static LuaValue LoadEnvironment(LuaValue[] args)
        {
            string fn = (args[0] as LuaString).Text;
            LuaRuntime.GlobalEnvironment = Serializer.Deserialize(fn) as LuaTable;
            return LuaNil.Nil;
        }
    }
}
