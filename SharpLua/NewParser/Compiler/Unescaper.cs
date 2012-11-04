using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.Compiler
{
    public static class Unescaper
    {
        public static string Unescape(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\')
                {
                    char cOld = c;
                    c = s[++i];
                    if (c == 'n')
                        sb.Append("\n");
                    else if (c == 'r')
                        sb.Append("\r");
                    else if (c == 't')
                        sb.Append("\t");
                    else if (c == 'a')
                        sb.Append("\a");
                    else if (c == 'b')
                        sb.Append("\b");
                    else if (c == 'f')
                        sb.Append("\f");
                    else if (c == 'v')
                        sb.Append("\v");
                    else
                    {
                        if (char.IsDigit(c) == false)
                            sb.Append(c);
                        else
                        {
                            //string num = c;
                            string num = "";
                            int got = 0;
                            while (char.IsDigit(c))
                            {
                                num += c;
                                c = s[++i];
                                got++;
                                if (got == 3)
                                    break;
                            }
                            sb.Append((char)int.Parse(num));
                        }
                    }
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
