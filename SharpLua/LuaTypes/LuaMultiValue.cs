using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    public class LuaMultiValue : LuaValue
    {
        public LuaMultiValue(LuaValue[] values)
        {
            this.Values = values;
        }

        public LuaValue[] Values { get; set; }

        public override object Value
        {
            get { return this.Values; }
        }

        public override string GetTypeCode()
        {
            throw new InvalidOperationException();
        }

        public static LuaValue WrapLuaValues(LuaValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return LuaNil.Nil;
            }
            else if (values.Length == 1)
            {
                return values[0];
            }
            else
            {
                return new LuaMultiValue(UnWrapLuaValues(values));
            }
        }

        public static LuaValue[] UnWrapLuaValues(LuaValue[] values)
        {
            if (values == null || values.Length == 0 || ContainsMultiValue(values) == false)
            {
                return values;
            }

            if (values.Length == 1 && values[0] is LuaMultiValue)
            {
                return (values[0] as LuaMultiValue).Values;
            }

            List<LuaValue> neatValues = new List<LuaValue>(values.Length);

            for (int i = 0; i < values.Length - 1; i++)
            {
                LuaValue value = values[i];
                LuaMultiValue multiValue = value as LuaMultiValue;

                if (multiValue != null)
                {
                    neatValues.Add(multiValue.Values[0]);
                }
                else
                {
                    neatValues.Add(value);
                }
            }

            LuaValue lastValue = values[values.Length - 1];
            LuaMultiValue lastMultiValue = lastValue as LuaMultiValue;

            if (lastMultiValue != null)
            {
                neatValues.AddRange(lastMultiValue.Values);
            }
            else
            {
                neatValues.Add(lastValue);
            }

            return neatValues.ToArray();
        }

        private static bool ContainsMultiValue(LuaValue[] values)
        {
            foreach (LuaValue value in values)
            {
                if (value is LuaMultiValue)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
