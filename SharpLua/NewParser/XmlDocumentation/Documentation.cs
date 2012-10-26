using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SharpLua.XmlDocumentation
{
    public static class Documentation
    {
        public static List<DocumentationComment> Read(string s)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            XmlNode n = doc.SelectSingleNode("/Documentation");
            List<DocumentationComment> ret = new List<DocumentationComment>();
            foreach (XmlNode n2 in n.ChildNodes)
            {
                DocumentationComment dc = new DocumentationComment();
                dc.Ident = n2.Name;
                string s2 = n2.InnerText;
                string[] lines = s2.Split('\n');
                foreach (string line in lines)
                    dc.Lines.Add(line.Replace("\r", "").Replace("\n", ""));
                ret.Add(dc);
            }
            return ret;
        }

        public static string Write(List<DocumentationComment> comments)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter w = XmlWriter.Create(sb, new XmlWriterSettings() { Indent = true, IndentChars = "    ", });
            w.WriteStartElement("Documentation");
            foreach (DocumentationComment cmt in comments)
            {
                w.WriteStartElement(cmt.Ident);
                w.WriteString(cmt.Text);
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.Flush();
            w.Close();
            return sb.ToString();
        }
    }
}
