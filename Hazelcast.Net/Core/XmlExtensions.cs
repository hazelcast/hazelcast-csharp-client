using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines extension methods for handling Xml.
    /// </summary>
    internal static class XmlExtensions
    {
        public static string GetCleanName(this XmlNode node)
        {
            return Regex.Replace(node.Name, "\\w+:", string.Empty).ToLower();
        }

        public static string GetStringAttribute(this XmlNode node, string name)
        {
            var attributes = node.Attributes;
            if (attributes == null) return null;

            foreach (XmlAttribute attribute in attributes)
                if (attribute.Name == name)
                    return attribute.Value;

            return null;
        }

        public static string GetTextContent(this XmlNode node)
        {
            return node != null ? node.InnerText.Trim() : string.Empty;
        }

        public static int GetInt32Content(this XmlNode node)
        {
            return Convert.ToInt32(node.GetTextContent());
        }

        public static bool GetBoolContent(this XmlNode node)
        {
            return bool.Parse(node.GetTextContent());
        }

        public static bool GetTrueFalseContent(this XmlNode node)
        {
            var value = node.GetTextContent();
            return "true".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                   "yes".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                   "on".Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static T GetEnumContent<T>(this XmlNode node)
            where T : struct
        {
            return Enum.TryParse(node.GetTextContent(), true, out T result) ? result : default;
        }

        public static void FillProperties(this XmlNode node, Dictionary<string, string> properties)
        {
            if (properties == null)
            {
                return;
            }
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Text || n.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }
                var value = GetTextContent(n).Trim();
                var name = n.GetCleanName();
                string propertyName;
                if ("property".Equals(name))
                {
                    var xmlAttributeCollection = n.Attributes;
                    if (xmlAttributeCollection != null)
                    {
                        propertyName = GetTextContent(xmlAttributeCollection.GetNamedItem("name")).Trim();
                        properties[propertyName] = value;
                    }
                }
                else
                {
                    // old way - probably should be deprecated
                    propertyName = name;
                    properties[propertyName] = value;
                }
            }
        }
    }
}
