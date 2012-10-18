using System.Text;
using System;
namespace SharpLua.LASM
{
    class Bit
    {
        //new = function(str)
        //	return tonumber(str, 2)
        //end,

        public static long Get(long num, int n)
        {
            double pn = Math.Pow(2, (n - 1));
            return (num % (pn + pn) >= pn) ? 1 : 0;
        }

        public static long Get(long num, int n, int n2)
        {
            long total = 0;
            long digitn = 0;
            for (int i = n; i < n2; i++)
            {
                total = total + (long)Math.Pow(2, digitn * Bit.Get(num, i));
                digitn = digitn + 1;
            }
            return total;
        }

        public static string GetString(long num, int mindigit = 0, string sep = "-")
        {
            int pow = 0;
            long tot = 1;
            while (tot <= num)
            {
                tot = tot * 2;
                pow = pow + 1;
            }
            if (pow < mindigit)
                pow = mindigit;

            string str = "";
            for (int i = pow; i > 1; i--)
                str = str + Bit.Get(num, i) + (i == 1 ? "" : sep);
            return str;
        }

        public static readonly int[] p2 = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072 };
        public static long keep(long x, int n)
        {
            return x % p2[n];
        }
        public static long srb(long x, int n)
        {
            return (long)Math.Floor((double)x / p2[n]);// + 1]);
        }
        public static long slb(long x, int n)
        {
            return x * p2[n];// + 1];
        }

        public static double ldexp(double val, int exp)
        {
            return (val * (Math.Pow(2, exp)));
        }

        public class FRexpResult
        {
            public int exponent = 0;
            public double mantissa = 0;
        }

        public static FRexpResult frexp(double value)
        {
            FRexpResult result = new FRexpResult();
            long bits = BitConverter.DoubleToInt64Bits(value);
            double realMant = 1;

            // Test for NaN, infinity, and zero. 
            if (Double.IsNaN(value) ||
                value + value == value ||
                Double.IsInfinity(value))
            {
                result.exponent = 0;
                result.mantissa = value;
            }
            else
            {
                bool neg = (bits < 0);
                int exponent = (int)((bits >> 52) & 0x7ffL);
                long mantissa = bits & 0xfffffffffffffL;

                if (exponent == 0)
                {
                    exponent++;
                }
                else
                {
                    mantissa = mantissa | (1L << 52);
                }

                // bias the exponent - actually biased by 1023. 
                // we are treating the mantissa as m.0 instead of 0.m 
                //  so subtract another 52. 
                exponent -= 1075;
                realMant = mantissa;

                // normalize 
                while (realMant > 1.0)
                {
                    mantissa >>= 1;
                    realMant /= 2;
                    exponent++;
                }

                if (neg)
                {
                    realMant = realMant * -1;
                }

                result.exponent = exponent;
                result.mantissa = realMant;
            }
            return result;
        }

    }
    /*
    class DumpBinary
    {
        // This is... bad. Only supports X86 Standard
        public static String DumpString(string s)
            if #s != 0 then
                return DumpBinary.Int32(#s+1)..s.."\0");
            else
                return "\0\0\0\0";
            end
        end,
        Integer = function(n)
            return DumpBinary.Int32(n)
        end,]]
        Int8 = function(n)
            return string.char(n)
        end,--[[
        Int16 = function(n)
            error("DumpBinary::Int16() Not Implemented")
        end,
        Int32 = function(x)
            local v = ""
            x = math.floor(x)
            if x >= 0 then
                for i = 1, 4 do
                v = v..string.char(x % 256)
                x = math.floor(x / 256)
                end
            else -- x < 0
                x = -x
                local carry = 1
                for i = 1, 4 do
                    local c = 255 - (x % 256) + carry
                    if c == 256 then c = 0; carry = 1 else carry = 0 end
                    v = v..string.char(c)
                    x = math.floor(x / 256)
                end
            end
            return v
        end,
        Float64 = function(x)
            local function grab_byte(v)
                return math.floor(v / 256), string.char(math.floor(v) % 256)
            end
            local sign = 0
            if x < 0 then sign = 1; x = -x end
            local mantissa, exponent = math.frexp(x)
            if x == 0 then -- zero
                mantissa, exponent = 0, 0
            elseif x == 1/0 then
                mantissa, exponent = 0, 2047
            else
                mantissa = (mantissa * 2 - 1) * math.ldexp(0.5, 53)
                exponent = exponent + 1022
            end
            local v, byte = "" -- convert to bytes
            x = mantissa
            for i = 1,6 do
                x, byte = grab_byte(x)
                v = v..byte -- 47:0
            end
            x, byte = grab_byte(exponent * 16 + x)
            v = v..byte -- 55:48
            x, byte = grab_byte(sign * 128 + x)
            v = v..byte -- 63:56
            return v
        end,]]
    }*/
    public class DumpBinary
    {
        public static string Opcode(Instruction op)
        {
            long c0, c1, c2, c3;
            if (op.OpcodeType == OpcodeType.AsBx)
            {
                long Bx = op.sBx + 131071;
                long C = Bit.keep(Bx, 9);
                long B = Bit.srb(Bx, 9);
                c0 = op.OpcodeNumber + Bit.slb(Bit.keep(op.A, 2), 6);
                c1 = Bit.srb(op.A, 2) + Bit.slb(Bit.keep(C, 2), 6);
                c2 = Bit.srb(C, 2) + Bit.slb(Bit.keep(B, 1), 7);
                c3 = Bit.srb(B, 1);
            }
            else if (op.OpcodeType == OpcodeType.ABx)
            {
                long C = Bit.keep(op.Bx, 9);
                long B = Bit.srb(op.Bx, 9);
                c0 = op.OpcodeNumber + Bit.slb(Bit.keep(op.A, 2), 6);
                c1 = Bit.srb(op.A, 2) + Bit.slb(Bit.keep(C, 2), 6);
                c2 = Bit.srb(C, 2) + Bit.slb(Bit.keep(B, 1), 7);
                c3 = Bit.srb(B, 1);
            }
            else
            {
                c0 = op.OpcodeNumber + Bit.slb(Bit.keep(op.A, 2), 6);
                c1 = Bit.srb(op.A, 2) + Bit.slb(Bit.keep(op.C, 2), 6);
                c2 = Bit.srb(op.C, 2) + Bit.slb(Bit.keep(op.B, 1), 7);
                c3 = Bit.srb(op.B, 1);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append((char)c0);
            sb.Append((char)c1);
            sb.Append((char)c2);
            sb.Append((char)c3);
            return sb.ToString();
        }
    }
}