using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.Parser
{
    public interface ParserInput<T>
    {
        int Length { get; }

        bool HasInput(int pos);

        T GetInputSymbol(int pos);

        T[] GetSubSection(int position, int length);

        string FormErrorMessage(int position, string message);
    }
}
