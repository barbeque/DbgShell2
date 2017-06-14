using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;

namespace DbgShell
{
    public static class Output
    {
        public const string
            Separator = "$$",
            DbgPrefix = "DBG",
            ScriptPrefix = "SCRIPT";

        private class LinkEntry
        {
            public string Text { get; set; }
            public string Link { get; set; }
        }
        private static ArrayList entries = new ArrayList();
        public static void Write(string text)
        {
            if ((entries.Count > 0) && (entries[entries.Count - 1] is string))
            {
                string s = (string)entries[entries.Count - 1];
                s += text;
                entries[entries.Count - 1] = s;
            }
            else
            {
                entries.Add(text);
            }
        }
        public static void AddDbgLink(string text, string dbgCommand)
        {
            LinkEntry entry = new LinkEntry { Text = text, Link = DbgPrefix + dbgCommand };
            entries.Add(entry);
        }
        public static void AddScriptLink(string text, string typeName, string methodName, params string[] list)
        {
            StringBuilder builder = new StringBuilder(ScriptPrefix);
            builder.Append(typeName);
            builder.Append(Separator);
            builder.Append(methodName);
            if (list != null)
            {
                foreach (string param in list)
                {
                    builder.Append(Separator);
                    builder.Append(param);
                }
            }
            LinkEntry entry = new LinkEntry { Text = text, Link = builder.ToString() };
            entries.Add(entry);
        }
        public static void Clear()
        {
            entries.Clear();
        }
        public static void WriteLine(string text)
        {
            Write(text + "\n");
        }

        public static string ToOutputString()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("entries");
            doc.AppendChild(node);
            foreach (object entry in entries)
            {
                XmlElement child = doc.CreateElement("child");
                if (entry is string)
                {
                    child.SetAttribute("text", (string)entry);
                }
                else if (entry is LinkEntry)
                {
                    LinkEntry linkEntry = (LinkEntry)entry;
                    child.SetAttribute("text", linkEntry.Text);
                    child.SetAttribute("link", linkEntry.Link);
                }
                node.AppendChild(child);
            }
            return doc.OuterXml;
        }
    }
}
