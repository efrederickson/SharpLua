using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.XmlDocumentation
{
    public class DocumentationComment
    {
        public List<string> Lines = new List<string>();
        public string Ident = null;
        public string EOL = "\r\n";

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string l in Lines)
                {
                    string line = l.TrimStart();
                    if (line.Length > 3 && line.Substring(0, 3) == "---")
                        line = line.Substring(3);
                    sb.Append(line);
                    sb.Append(EOL);
                }
                return sb.ToString();
            }
        }

    }
}
