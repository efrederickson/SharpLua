using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpLua.XmlDocumentation;
using ICSharpCode.Core;
using System.Xml;
using System.Windows.Forms;

namespace SharpLuaAddIn
{
    class DocumentationManager
    {
        static Dictionary<string, DocumentationComment> documentation = new Dictionary<string, DocumentationComment>();

        static DocumentationManager()
        {
            try
            {
                // Load from doc.xml
                string path = typeof(DocumentationManager).Assembly.Location;
                path = Path.GetDirectoryName(path);
                path += "/Documentation.xml";
                if (File.Exists(path))
                {
                    foreach (DocumentationComment dc in Documentation.Read(File.ReadAllText(path)))
                    {
                        documentation.Add(dc.Ident, dc);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.Error("Could not load default documentation symbols: ", ex);
            }
        }

        public static string GetDocumentation(string field)
        {
            if (documentation.ContainsKey(field))
                return generate(documentation[field]);
            else
                return "";
        }

        static string generate(DocumentationComment dc)
        {
            try
            {
                XmlDocument d = new XmlDocument();
                d.LoadXml("<doc>" + dc.Text + "</doc>");
                StringBuilder sb = new StringBuilder();
                if (d.SelectNodes("/doc/param").Count > 0)
                {
                    sb.Append(dc.Ident + "(");
                    //foreach (XmlNode n in d.SelectNodes("/doc/param"))
                    for (int i = 0; i < d.SelectNodes("/doc/param").Count; i++)
                    {
                        sb.Append(d.SelectNodes("/doc/param")[i].Attributes["name"].InnerText.Trim());
                        if (i != d.SelectNodes("/doc/param").Count - 1)
                            sb.Append(", ");
                    }
                    sb.Append(")");
                    sb.AppendLine();
                    sb.AppendLine();
                }
                sb.AppendLine(d.SelectSingleNode("/doc/summary").InnerText.Trim().Replace("<br />", "\r\n"));
                foreach (XmlNode n in d.SelectNodes("/doc/param"))
                {
                    sb.Append(n.Attributes["name"].InnerText.Trim());
                    sb.Append(": ");
                    sb.AppendLine(n.InnerText.Trim());
                }
                foreach (XmlNode n in d.SelectNodes("/doc/returns"))
                {
                    sb.AppendLine("Returns: " + n.InnerText.Trim());
                }
                sb.Remove(sb.Length - 1, 1); // remove last \n
                return sb.ToString();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                LoggingService.Error(dc.Text, ex);
            }
            return dc.Text;
        }
    }
}
