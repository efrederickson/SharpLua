using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public static class StringLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("string", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("byte", @byte);
            module.Register("char", @char);
            module.Register("format", format);
            module.Register("len", len);
            module.Register("sub", sub);
            module.Register("lower", lower);
            module.Register("upper", upper);
            module.Register("rep", rep);
            module.Register("reverse", reverse);
            module.Register("find", Find);
        }

        public static LuaValue @byte(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            LuaNumber startNumber = values.Length > 1 ? values[1] as LuaNumber : null;
            LuaNumber endNumber = values.Length > 2 ? values[2] as LuaNumber : null;

            int start = startNumber == null ? 1 : (int)startNumber.Number;
            int end = endNumber == null ? start : (int)endNumber.Number;

            LuaValue[] numbers = new LuaValue[end - start + 1];
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = new LuaNumber (char.ConvertToUtf32(str.Text, start - 1 + i) );
            }

            return new LuaMultiValue(numbers);
        }

        public static LuaValue @char(LuaValue[] values)
        {
            char[] chars = new char[values.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                int number = (int)(values[i] as LuaNumber).Number;
                chars[i] = (char)number;
            }

            return new LuaString(new string(chars));
        }

        public static LuaValue format(LuaValue[] values)
        {
            LuaString format = values[0] as LuaString;
            object[] args = new object[values.Length - 1];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = values[i + 1].Value;
            }
            return new LuaString(string.Format(format.Text, args));
        }

        public static LuaValue sub(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            LuaNumber startNumber = values[1] as LuaNumber;
            LuaNumber endNumber = values.Length > 2 ? values[2] as LuaNumber : null;

            int start = (int)startNumber.Number;
            int end = endNumber == null ? -1 : (int)endNumber.Number;

            if (start < 0)
            {
                start = str.Text.Length + start + 1;
            }
            if (end < 0)
            {
                end = str.Text.Length + end + 1;
            }

            return new LuaString(str.Text.Substring(start - 1, end - start + 1));
        }

        public static LuaValue rep(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            LuaNumber number = values[1] as LuaNumber;
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < number.Number; i++)
            {
                text.Append(str.Text);
            }
            return new LuaString(text.ToString());
        }

        public static LuaValue reverse(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            char[] chars = str.Text.ToCharArray();
            Array.Reverse(chars);
            return new LuaString(new string(chars));
        }

        public static LuaValue len(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            return new LuaNumber(str.Text.Length);
        }

        public static LuaValue lower(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            return new LuaString(str.Text.ToLower());
        }

        public static LuaValue upper(LuaValue[] values)
        {
            LuaString str = values[0] as LuaString;
            return new LuaString(str.Text.ToUpper());
        }
        
        public static LuaValue Find(LuaValue[] args)
        {
            string s = (args[0] as LuaString).Text;
            string format = (args[1] as LuaString).Text;
            int init = 0;
            if (args.Length > 2)
                init = (int.Parse((args[2] as LuaNumber).Number.ToString()) - 1) > 0 ? (int.Parse((args[2] as LuaNumber).Number.ToString()) - 1) : 0;
            bool plain = false;
            if (args.Length > 3)
                plain = (args[3] as LuaBoolean).BoolValue;
            // TODO: return captures
            if (plain)
            {
                int start = s.IndexOf(format);
                int end = format.Length;
                return new LuaMultiValue(new LuaValue[] { new LuaNumber(start), new LuaNumber(end), LuaNil.Nil});
            }
            else
            {
                //TODO: format->regex
                return LuaNil.Nil;
            }
        }
    }
}
