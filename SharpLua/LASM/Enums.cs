using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LASM
{
    public enum Vararg
    {
        VARARG_NOVARARG = 0,
        VARARG_HASARG = 1,
        VARARG_ISVARARG = 2,
        VARARG_NEEDSARG = 4,
    }

    public enum Format
    {
        Official,
        Unofficial,
    }

    public enum ConstantType
    {
        Nil = 0,
        Bool = 1,
        Number = 3,
        String = 4,
    }

    public enum OpcodeType
    {
        ABC, //ABC
        ABx, //ABx
        AsBx //AsBx
    };
}