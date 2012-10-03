using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public static class MathLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("math", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.SetNameValue("huge", new LuaNumber(double.MaxValue));
            module.SetNameValue("pi", new LuaNumber(Math.PI));
            module.Register("abs", Abs);
            module.Register("acos", Acos);
            module.Register("asin", Asin);
            module.Register("atan", Atan);
            module.Register("atan2", Atan2);
            module.Register("ceil", Ceil);
            module.Register("cos", Cos);
            module.Register("cosh", Cosh);
            module.Register("deg", Deg);
            module.Register("exp", Exp);
            module.Register("floor", Floor);
            module.Register("fmod", Fmod);
            module.Register("log", Log);
            module.Register("log10", Log10);
            module.Register("max", Max);
            module.Register("min", Min);
            module.Register("modf", ModF);
            module.Register("pow", Pow);
            module.Register("rad", Rad);
            module.Register("random", Random);
            module.Register("randomseed", RandomSeed);
            module.Register("sin", Sin);
            module.Register("sinh", SinH);
            module.Register("sqrt", Sqrt);
            module.Register("tan", Tan);
            module.Register("tanh", TanH);
        }

        public static LuaValue Abs(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Abs(number.Number));
        }

        public static LuaValue Acos(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Acos(number.Number));
        }

        public static LuaValue Asin(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Asin(number.Number));
        }

        public static LuaValue Atan(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Atan(number.Number));
        }

        public static LuaValue Atan2(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.Atan2(numbers.Item1, numbers.Item2));
        }

        public static LuaValue Ceil(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Ceiling(number.Number));
        }

        public static LuaValue Cos(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Cos(number.Number));
        }

        public static LuaValue Cosh(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Cosh(number.Number));
        }

        public static LuaValue Deg(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(number.Number * 180 / Math.PI);
        }

        public static LuaValue Exp(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Exp(number.Number));
        }

        public static LuaValue Floor(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Floor(number.Number));
        }

        public static LuaValue Fmod(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.IEEERemainder(numbers.Item1, numbers.Item2));
        }

        public static LuaValue Log(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Log(number.Number));
        }

        public static LuaValue Log10(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Log10(number.Number));
        }

        public static LuaValue Max(LuaValue[] values)
        {
            return new LuaNumber(values.Max(v => (v as LuaNumber).Number));
        }

        public static LuaValue Min(LuaValue[] values)
        {
            return new LuaNumber(values.Min(v => (v as LuaNumber).Number));
        }

        public static LuaValue ModF(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            double integer = Math.Floor(number.Number);
            return new LuaMultiValue(new LuaNumber[]
                {
                 new LuaNumber(integer),
                 new LuaNumber(number.Number - integer)
                }
            );
        }

        public static LuaValue Pow(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.Pow(numbers.Item1, numbers.Item2));
        }

        public static LuaValue Rad(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(number.Number * Math.PI / 180);
        }

        static Random randomGenerator = new Random();
        public static LuaValue Random(LuaValue[] values)
        {
            if (values.Length == 0)
            {
                return new LuaNumber(randomGenerator.NextDouble());
            }
            else if (values.Length == 1)
            {
                LuaNumber number1 = values[0] as LuaNumber;
                return new LuaNumber(randomGenerator.Next((int)number1.Number));
            }
            else
            {
                var numbers = CheckArgs2(values);
                return new LuaNumber(randomGenerator.Next((int)numbers.Item1, (int)numbers.Item2));
            }
        }

        public static LuaValue RandomSeed(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            randomGenerator = new Random((int)number.Number);
            return number;
        }

        public static LuaValue Sin(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sin(number.Number));
        }

        public static LuaValue SinH(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sinh(number.Number));
        }

        public static LuaValue Sqrt(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sqrt(number.Number));
        }

        public static LuaValue Tan(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Tan(number.Number));
        }

        public static LuaValue TanH(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Tanh(number.Number));
        }

        private static LuaNumber CheckArgs(LuaValue[] values)
        {
            if (values.Length >= 1)
            {
                LuaNumber number = values[0] as LuaNumber;
                if (number != null)
                {
                    return number;
                }
                else
                {
                    throw new LuaError("bad argument #1 to 'abs' (number expected, got {0})", values[0].GetTypeCode());
                }
            }
            else
            {
                throw new LuaError("bad argument #1 to 'abs' (number expected, got no value)");
            }
        }

        private static Tuple<double, double> CheckArgs2(LuaValue[] values)
        {
            if (values.Length >= 2)
            {
                LuaNumber number1 = values[0] as LuaNumber;
                if (number1 == null)
                {
                    throw new LuaError("bad argument #1 to 'abs' (number expected, got {0})", values[0].GetTypeCode());
                }

                LuaNumber number2 = values[1] as LuaNumber;
                if (number2 == null)
                {
                    throw new LuaError("bad argument #2 to 'abs' (number expected, got {0})", values[1].GetTypeCode());
                }

                return Tuple.Create(number1.Number, number2.Number);
            }
            else
            {
                throw new LuaError("bad argument #1 to 'abs' (number expected, got no value)");
            }
        }
    }
}
