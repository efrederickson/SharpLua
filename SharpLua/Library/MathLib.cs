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
            module.Register("abs", abs);
            module.Register("acos", acos);
            module.Register("asin", asin);
            module.Register("atan", atan);
            module.Register("atan2", atan2);
            module.Register("ceil", ceil);
            module.Register("cos", cos);
            module.Register("cosh", cosh);
            module.Register("deg", deg);
            module.Register("exp", exp);
            module.Register("floor", floor);
            module.Register("fmod", fmod);
            module.Register("log", log);
            module.Register("log10", log10);
            module.Register("max", max);
            module.Register("min", min);
            module.Register("modf", modf);
            module.Register("pow", pow);
            module.Register("rad", rad);
            module.Register("random", random);
            module.Register("randomseed", randomseed);
            module.Register("sin", sin);
            module.Register("sinh", sinh);
            module.Register("sqrt", sqrt);
            module.Register("tan", tan);
            module.Register("tanh", tanh);
        }

        public static LuaValue abs(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Abs(number.Number));
        }

        public static LuaValue acos(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Acos(number.Number));
        }

        public static LuaValue asin(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Asin(number.Number));
        }

        public static LuaValue atan(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Atan(number.Number));
        }

        public static LuaValue atan2(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.Atan2(numbers.Item1, numbers.Item2));
        }

        public static LuaValue ceil(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Ceiling(number.Number));
        }

        public static LuaValue cos(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Cos(number.Number));
        }

        public static LuaValue cosh(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Cosh(number.Number));
        }

        public static LuaValue deg(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(number.Number * 180 / Math.PI);
        }

        public static LuaValue exp(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Exp(number.Number));
        }

        public static LuaValue floor(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Floor(number.Number));
        }

        public static LuaValue fmod(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.IEEERemainder(numbers.Item1, numbers.Item2));
        }

        public static LuaValue log(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Log(number.Number));
        }

        public static LuaValue log10(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Log10(number.Number));
        }

        public static LuaValue max(LuaValue[] values)
        {
            return new LuaNumber(values.Max(v => (v as LuaNumber).Number));
        }

        public static LuaValue min(LuaValue[] values)
        {
            return new LuaNumber(values.Min(v => (v as LuaNumber).Number));
        }

        public static LuaValue modf(LuaValue[] values)
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

        public static LuaValue pow(LuaValue[] values)
        {
            var numbers = CheckArgs2(values);
            return new LuaNumber(Math.Pow(numbers.Item1, numbers.Item2));
        }

        public static LuaValue rad(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(number.Number * Math.PI / 180);
        }

        static Random randomGenerator = new Random();
        public static LuaValue random(LuaValue[] values)
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

        public static LuaValue randomseed(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            randomGenerator = new Random((int)number.Number);
            return number;
        }

        public static LuaValue sin(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sin(number.Number));
        }

        public static LuaValue sinh(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sinh(number.Number));
        }

        public static LuaValue sqrt(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Sqrt(number.Number));
        }

        public static LuaValue tan(LuaValue[] values)
        {
            LuaNumber number = CheckArgs(values);
            return new LuaNumber(Math.Tan(number.Number));
        }

        public static LuaValue tanh(LuaValue[] values)
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
