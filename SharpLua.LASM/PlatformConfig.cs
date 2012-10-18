// Mostly from ChunkSpy. Used in loading chunks and converting them to different platforms.
using System;
using System.Collections.Generic;
namespace SharpLua.LASM
{
public class PlatformConfig
{
    public class Platform
    {
        public string Description;
        public bool BigEndian;
        public int IntegerSize;
        public int SizeT;
        public int InstructionSize;
        public int NumberSize;
        public bool IsFloatingPoint;
        public string NumberType;
    }

    const int luaNumberSize = 8; // TODO

static Dictionary<string, Platform> Configuration = new Dictionary<string, Platform>();

    static PlatformConfig()
    {
    Configuration.Add("x86 standard", new Platform{
        Description = "x86 Standard (32-bit, little endian, double)",    
        BigEndian = false,             // 1 = little endian    
        IntegerSize = 4,                // (data type sizes in bytes)    
        SizeT = 4,    
        InstructionSize = 4,    
        NumberSize = 8,                 // this & integral identifies the    
        IsFloatingPoint = true,        // (0) type of lua_Number    
        NumberType = "double",         // used for lookups  
    });
    Configuration.Add("big endian int", new Platform{
        Description = "(32-bit, big endian, int)",    
        BigEndian = true,
        IntegerSize = 4,
        SizeT = 4,
        InstructionSize = 4,
        NumberSize = 4,
        IsFloatingPoint = false,
        NumberType = "int",  
    });
    Configuration.Add("amd64", new Platform{
        Description = "AMD64 (64-bit, little endian, double)",    
        BigEndian = false, 
        IntegerSize = 4,    
        SizeT = 8,    
        InstructionSize = 4,    
        NumberSize = 8,    
        IsFloatingPoint = true,
        NumberType = "double",  
    });
    // you can add more platforms here

        LuaNumberID.Add("80", "double");         // IEEE754 double
  LuaNumberID.Add("40", "single");         // IEEE754 single
  LuaNumberID.Add("41", "int");            // int
  LuaNumberID.Add("81", "long long");      // long long

        ConvertFrom.Add("double", fromDouble);
        ConvertFrom.Add("single", fromSingle);
        ConvertFrom.Add("int", fromInt);
        ConvertFrom.Add("long long", fromInt);
        ConvertTo.Add("double", toDouble);
        ConvertTo.Add("int", toInt);
        ConvertTo.Add("long long", toInt);
}

static void grab_byte(double v, out double x, out char c)
{
  x = Math.Floor(v / 256);
    c = (char)(Math.Floor(v) % 256);
}

///////////////////////////////////////////////////////////////////////
// No more TEST_NUMBER in Lua 5.1, uses size_lua_Number + integral
// LuaFile.IntegerSize .. (LuaFile.IsFloatingPoint and 0 or 1)
///////////////////////////////////////////////////////////////////////
static Dictionary<string, string> LuaNumberID = new Dictionary<string, string>();
    
static Dictionary<string, Func<string, double>> ConvertFrom = new Dictionary<string, Func<string, double>>();
static Dictionary<string, Func<double, string>> ConvertTo = new Dictionary<string, Func<double, string>>();

// Converts an 8-byte little-endian string to a IEEE754 double number
static double fromDouble(string x)
{
  int sign = 1;
  double mantissa = (int)x[6] % 16;
  for (int i = 6; i > 0; i--)
      mantissa = mantissa * 256 + (int)x[i];
  if ((int)x[7] > 127)
      sign = -1;
  int exponent = (int)(((int)x[7] % 128) * 16 +
                   Math.Floor((double)(int)x[6] / 16));
  if (exponent == 0)
      return 0;
  mantissa = (Bit.ldexp(mantissa, -52) + 1) * sign;
  return Bit.ldexp(mantissa, exponent - 1023);
}

// Converts a 4-byte little-endian string to a IEEE754 single number
static double fromSingle(string x)
{
  int sign = 1;
  double mantissa = (int)x[2] % 128;
      mantissa = mantissa * 256 + (int)x[1];
    mantissa = mantissa * 256 + (int)x[0];
  if ((int)x[3] > 127)
      sign = -1;
  int exponent = (int)(((int)x[3] % 128) * 2 +
                   Math.Floor((double)(int)x[2] / 128));
  if (exponent == 0)
      return 0;
  mantissa = (Bit.ldexp(mantissa, -23) + 1) * sign;
  return Bit.ldexp(mantissa, exponent - 127);
}

// Converts a little-endian integer string to a number
static double fromInt(string x)
{
  double sum = 0;
  for (int i = luaNumberSize; i > 1; i--)
    sum = sum * 256 + (int)x[i];
  // test for negative number
  if (x[luaNumberSize] > 127) 
    sum = sum - Bit.ldexp(1, 8 * luaNumberSize);
  
  return sum;
}

// WARNING this will fail for large long longs (64-bit numbers)
// because long longs exceeds the precision of doubles.
// ConvertFrom["long long"] = ConvertFrom["int"]

// Converts a IEEE754 double number to an 8-byte little-endian string
static string toDouble(double x)
{
 int sign = 0;
  if (x < 0)
  {
      sign = 1; 
      x = -x;
}
    Bit.FRexpResult fr = Bit.frexp(x);
  if (x == 0)// zero
  {
      fr.mantissa = 0;
    fr.exponent = 0;
}
  else
  {
    fr.mantissa = (fr.mantissa * 2 - 1) * Bit.ldexp(0.5, 53);
    fr.exponent = fr.exponent + 1022;
  }
  string v = "";
    char b = '\0';// convert to bytes
  x = fr.mantissa;
  for (int i = 0; i < 6; i++)
  {
      x = Math.Floor(x / 256);
    b = (char)(Math.Floor(x) % 256);
      v = v + b;
    // 47:0
  }
    x = Math.Floor((fr.exponent * 16 + x) / 256);
    b = (char)(Math.Floor((fr.exponent * 16 + x)) % 256);
    v += b;
  // 55:48
    x = Math.Floor((sign * 128 + x) / 256);
    b = (char)(Math.Floor((sign * 128 + x) % 256));
    v += b;
  // 63:56
  return v;
}

// Converts a IEEE754 single number to a 4-byte little-endian string
    static string toSingle(double x)
    {
  int sign = 0;
  if (x < 0) 
  {
      sign = 1; 
      x = -x; 
  }
  Bit.FRexpResult fr = Bit.frexp(x);
  if (x == 0) // zero
  {
    fr.mantissa = 0;
      fr.exponent = 0;
  }
  else
  {
   fr. mantissa = (fr.mantissa * 2 - 1) * Bit.ldexp(0.5, 24);
    fr.exponent = fr.exponent + 126;
  }
      string v = "";
        char b = '\0';
  grab_byte(fr.mantissa, out x, out b); 
        v = v+ b; // 7:0
  grab_byte(x, out x, out b); 
        v = v+b;// 15:8
  grab_byte(fr.exponent * 128 + x, out x, out b); 
        v = v+b;// 23:16
  grab_byte(sign * 128 + x, out x, out b);
        v = v+b; // 31:24
  return v;
    }

// Converts a number to a little-endian integer string
static string toInt(double x)
{
  string v = "";
  x = Math.Floor(x);
  if (x >= 0)
  {
    for (int i = 0; i <  luaNumberSize; i++)
    {
      v = v+ (char)(x % 256); 
      x = Math.Floor(x / 256);
    }
  }
  else// x < 0
  {
    x = -x;
    int carry = 1;
    for (int i = 0; i< luaNumberSize; i++)
    {
      double c = 255 - (x % 256) + carry;
      if (c == 256)
      {
          c = 0; 
        carry = 1;
      }
        else 
        carry = 0;
      v = v+ (char)c;
        x = Math.Floor(x / 256);
    }
  }
  // optional overflow test; not enabled at the moment
  // if x > 0 then error("number conversion overflow") end
  return v;
}

// WARNING this will fail for large long longs (64-bit numbers)
// because long longs exceeds the precision of doubles.
//ConvertTo["long long"] = ConvertTo["int"];

public static Func<string, double> GetNumberTypeConvertFrom(LuaFile file)
{
    string nt = LuaNumberID[file.IntegerSize.ToString() + (file.IsFloatingPointNumbers? "0" : "1")];
    if (nt == null)
        throw new Exception("Unable to determine Number type");
    return ConvertFrom[nt];
}

public static Func<double, string> GetNumberTypeConvertTo(LuaFile file)
{
    string nt = LuaNumberID[file.NumberSize.ToString() + (file.IsFloatingPointNumbers ? "0" : "1")];
    if (nt == null)
        throw new Exception("Unable to determine Number type");
    return ConvertTo[nt];
}
}
}