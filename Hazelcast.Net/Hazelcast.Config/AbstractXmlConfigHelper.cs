// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Config
{
    public abstract class AbstractXmlConfigHelper
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (AbstractXmlConfigHelper));

        public string CleanNodeName(XmlNode node)
        {
            return CleanNodeName(node.Name);
        }

        public static string CleanNodeName(string nodeName)
        {
            var name = nodeName;
            if (name != null)
            {
                name = Regex.Replace(nodeName, "\\w+:", string.Empty).ToLower();
            }
            return name;
        }

        protected internal virtual bool CheckTrue(string value)
        {
            return
                "true".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                "yes".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                "on".Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        protected internal void FillDataSerializableFactories(XmlNode node, SerializationConfig serializationConfig)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                if ("data-serializable-factory".Equals(name))
                {
                    var value = GetTextContent(child);
                    var factoryIdNode = child.Attributes.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException(
                            "'factory-id' attribute of 'data-serializable-factory' is required!");
                    }
                    var factoryId = Convert.ToInt32(GetTextContent(factoryIdNode));
                    serializationConfig.AddDataSerializableFactoryClass(factoryId, value);
                }
            }
        }

        protected internal virtual
            void FillPortableFactories
            (XmlNode node, SerializationConfig serializationConfig)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                if ("portable-factory".Equals(name))
                {
                    var value = GetTextContent(child);
                    var factoryIdNode = child.Attributes.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException("'factory-id' attribute of 'portable-factory' is required!");
                    }
                    var factoryId = Convert.ToInt32(GetTextContent(factoryIdNode));
                    serializationConfig.AddPortableFactoryClass(factoryId, value);
                }
            }
        }

        protected internal virtual void FillProperties(XmlNode node, Dictionary<string, string> properties)
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
                var name = CleanNodeName(n.Name);
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

        protected internal virtual
            void FillSerializers
            (XmlNode node, SerializationConfig serializationConfig)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                var value = GetTextContent(child);
                if ("serializer".Equals(name))
                {
                    var serializerConfig = new SerializerConfig();
                    serializerConfig.SetClassName(value);
                    var typeClassName = GetAttribute(child, "type-class");
                    serializerConfig.SetTypeClassName(typeClassName);
                    serializationConfig.AddSerializerConfig(serializerConfig);
                }
                else
                {
                    if ("global-serializer".Equals(name))
                    {
                        var globalSerializerConfig = new GlobalSerializerConfig();
                        globalSerializerConfig.SetClassName(value);
                        serializationConfig.SetGlobalSerializerConfig(globalSerializerConfig);
                    }
                }
            }
        }

        protected internal virtual string GetAttribute(XmlNode node, string attName)
        {
            var xmlAttributeCollection = node.Attributes;
            if (xmlAttributeCollection != null)
            {
                var attNode = xmlAttributeCollection.GetNamedItem(attName);
                if (attNode != null)
                {
                    return GetTextContent(attNode);
                }
            }
            return null;
        }

        protected internal virtual int GetIntegerValue(string parameterName, string value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception e)
            {
                Logger.Info(parameterName + " parameter value, [" + value +
                            "], is not a proper integer. Default value, [" + defaultValue + "], will be used!");
                Logger.Warning(e);
                return defaultValue;
            }
        }

        protected internal virtual string GetTextContent(XmlNode node)
        {
            if (node != null)
            {
                return node.InnerText.Trim();
            }
            return string.Empty;
        }

        protected internal virtual SerializationConfig ParseSerialization(XmlNode node)
        {
            var serializationConfig = new SerializationConfig();
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                switch (name)
                {
                    case "portable-version":
                        serializationConfig.SetPortableVersion(GetIntegerValue(name, GetTextContent(child), 0));
                        break;
                    case "check-class-def-errors":
                        serializationConfig.SetCheckClassDefErrors(CheckTrue(GetTextContent(child)));
                        break;
                    case "use-native-byte-order":
                        serializationConfig.SetUseNativeByteOrder(CheckTrue(GetTextContent(child)));
                        break;
                    case "byte-order":
                        var bigEndian = GetTextContent(child);
                        var byteOrder = ByteOrder.GetByteOrder(bigEndian);
                        serializationConfig.SetByteOrder(byteOrder);
                        break;
                    case "enable-compression":
                        serializationConfig.SetEnableCompression(CheckTrue(GetTextContent(child)));
                        break;
                    case "enable-shared-object":
                        serializationConfig.SetEnableSharedObject(CheckTrue(GetTextContent(child)));
                        break;
                    case "data-serializable-factories":
                        FillDataSerializableFactories(child, serializationConfig);
                        break;
                    case "portable-factories":
                        FillPortableFactories(child, serializationConfig);
                        break;
                    case "serializers":
                        FillSerializers(child, serializationConfig);
                        break;
                }
            }
            return serializationConfig;
        }

        protected internal virtual SocketInterceptorConfig ParseSocketInterceptorConfig(XmlNode node)
        {
            var socketInterceptorConfig = new SocketInterceptorConfig();
            var atts = node.Attributes;
            var enabledNode = atts.GetNamedItem("enabled");
            var enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            socketInterceptorConfig.SetEnabled(enabled);
            foreach (XmlNode n in node.ChildNodes)
            {
                var nodeName = CleanNodeName(n.Name);
                if ("class-name".Equals(nodeName))
                {
                    socketInterceptorConfig.SetClassName(GetTextContent(n).Trim());
                }
                else
                {
                    if ("properties".Equals(nodeName))
                    {
                        FillProperties(n, socketInterceptorConfig.GetProperties());
                    }
                }
            }
            return socketInterceptorConfig;
        }
    }
}