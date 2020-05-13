// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Xml;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Configuration
{
    public abstract class XmlConfigHelperBase
    {
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

        public static bool CheckTrue(string value)
        {
            return
                "true".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                "yes".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                "on".Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        protected internal void FillDataSerializableFactories(XmlNode node, SerializationConfiguration serializationConfiguration)
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
                    serializationConfiguration.AddDataSerializableFactoryClass(factoryId, value);
                }
            }
        }

        protected internal virtual
            void FillPortableFactories
            (XmlNode node, SerializationConfiguration serializationConfiguration)
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
                    serializationConfiguration.AddPortableFactoryClass(factoryId, value);
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
            (XmlNode node, SerializationConfiguration serializationConfiguration)
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
                    serializationConfiguration.AddSerializerConfig(serializerConfig);
                }
                else
                {
                    if ("global-serializer".Equals(name))
                    {
                        var globalSerializerConfig = new GlobalSerializerConfig();
                        globalSerializerConfig.SetClassName(value);
                        serializationConfiguration.SetGlobalSerializerConfig(globalSerializerConfig);
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
                throw new ConfigurationException($"Invalid integer value {value}.");
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

        protected internal virtual void ParseSerialization(SerializationConfiguration serializationConfig, XmlNode node)
        {
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
                        var endianness = "BIG_ENDIAN".Equals(bigEndian) ? Endianness.BigEndian : Endianness.LittleEndian;
                        serializationConfig.SetEndianness(endianness);
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
        }

        protected internal virtual void ParseSocketInterceptorConfiguration(SocketInterceptorConfiguration socketInterceptorConfig, XmlNode node)
        {
            var atts = node.Attributes;
            var enabledNode = atts.GetNamedItem("enabled");
            var enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            socketInterceptorConfig.IsEnabled = enabled;
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
        }

        protected internal virtual void ParseSslConfiguration(SslConfiguration sslConfig, XmlNode node)
        {
            var atts = node.Attributes;
            var enabledNode = atts.GetNamedItem("enabled");
            var enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            sslConfig.IsEnabled = enabled;
            foreach (XmlNode n in node.ChildNodes)
            {
                var nodeName = CleanNodeName(n.Name);
                if ("properties".Equals(nodeName))
                {
                    var props = new Dictionary<string, string>();
                    FillProperties(props, n);
                    if (props.TryGetValue(HazelcastProperties.CertificateName, out var certificateName))
                    {
                        sslConfig.CertificateName = certificateName;
                    }
                    if (props.TryGetValue(HazelcastProperties.ValidateCertificateChain, out var validateCertificateChain))
                    {
                        sslConfig.ValidateCertificateChain = Convert.ToBoolean(validateCertificateChain);
                    }
                    if (props.TryGetValue(HazelcastProperties.ValidateCertificateName, out var validateCertificateName))
                    {
                        sslConfig.ValidateCertificateName = Convert.ToBoolean(validateCertificateName);
                    }
                    if (props.TryGetValue(HazelcastProperties.CheckCertificateRevocation, out var checkCertificateRevocation))
                    {
                        sslConfig.CheckCertificateRevocation = Convert.ToBoolean(checkCertificateRevocation);
                    }
                    if (props.TryGetValue(HazelcastProperties.CertificateFilePath, out var certificateFilePath))
                    {
                        sslConfig.CertificatePath = certificateFilePath;
                    }
                    if (props.TryGetValue(HazelcastProperties.CertificatePassword, out var certificatePassword))
                    {
                        sslConfig.CertificatePassword = certificatePassword;
                    }
                    if (props.TryGetValue(HazelcastProperties.SslProtocol, out var sslProtocol))
                    {
                        sslConfig.SslProtocol = (SslProtocols)Enum.Parse(typeof(SslProtocols), sslProtocol, true);
                    }
                }
            }
        }

        private void FillProperties(IDictionary<string, string> properties, XmlNode node)
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

        public class HazelcastProperty
        {
            public HazelcastProperty(string name, string defaultValue)
            {
                Name = name;
                DefaultValue = defaultValue;
            }

            public string Name { get; }

            public string DefaultValue { get; }
        }

        public class HazelcastProperties
        {
            public static readonly HazelcastProperty ShuffleMemberList =
                new HazelcastProperty("hazelcast.client.shuffle.member.list", "true");

            public static readonly HazelcastProperty HeartbeatTimeout =
                new HazelcastProperty("hazelcast.client.heartbeat.timeout", "60000");

            public static readonly HazelcastProperty HeartbeatInterval =
                new HazelcastProperty("hazelcast.client.heartbeat.interval", "5000");

            public static readonly HazelcastProperty EventThreadCount =
                new HazelcastProperty("hazelcast.client.event.thread.count", "5");

            public static readonly HazelcastProperty EventQueueCapacity =
                new HazelcastProperty("hazelcast.client.event.queue.capacity", "1000000");

            public static readonly HazelcastProperty InvocationTimeoutSeconds =
                new HazelcastProperty("hazelcast.client.invocation.timeout.seconds", "120");


            public static readonly HazelcastProperty ConfigFilePath = new HazelcastProperty("hazelcast.client.config", ".");

            public static readonly HazelcastProperty LoggingLevel = new HazelcastProperty("hazelcast.logging.level", "all");

            public static readonly HazelcastProperty LoggingType = new HazelcastProperty("hazelcast.logging.type", "none");


            public static readonly HazelcastProperty CloudUrlBase =
                new HazelcastProperty("hazelcast.client.cloud.url", "https://coordinator.hazelcast.cloud");


            public static readonly HazelcastProperty ReconciliationIntervalSecondsProperty =
                new HazelcastProperty("hazelcast.invalidation.reconciliation.interval.seconds", "60");

            public static readonly HazelcastProperty MinReconciliationIntervalSecondsProperty =
                new HazelcastProperty("hazelcast.invalidation.min.reconciliation.interval.seconds", "30");

            public static readonly HazelcastProperty MaxToleratedMissCountProperty =
                new HazelcastProperty("hazelcast.invalidation.max.tolerated.miss.count", "10");


            /// <summary>
            /// Certificate Name to be validated against SAN field of the remote certificate, if not present then the CN part of the Certificate Subject.
            /// </summary>
            public static readonly string CertificateName = "CertificateServerName";

            /// <summary>
            /// Certificate File path.
            /// </summary>
            public static readonly string CertificateFilePath = "CertificateFilePath";

            /// <summary>
            /// Password need to import the certificates.
            /// </summary>
            public static readonly string CertificatePassword = "CertificatePassword";

            /// <summary>
            /// SSL/TLS protocol. string value of enum type <see cref="System.Security.Authentication.SslProtocols"/>
            /// </summary>
            public static readonly string SslProtocol = "SslProtocol";

            /// <summary>
            /// specifies whether the certificate revocation list is checked during authentication.
            /// </summary>
            public static readonly string CheckCertificateRevocation = "CheckCertificateRevocation";

            /// <summary>
            /// The property is used to configure ssl to enable certificate chain validation.
            /// </summary>
            public static readonly string ValidateCertificateChain = "ValidateCertificateChain";

            /// <summary>
            /// The property is used to configure ssl to enable Certificate name validation
            /// </summary>
            public static readonly string ValidateCertificateName = "ValidateCertificateName";


            internal IReadOnlyDictionary<string, string> Properties { get; }

            internal HazelcastProperties(IDictionary<string, string> properties)
            {
                Properties = new ReadOnlyDictionary<string, string>(properties);
            }

            internal string StringValue(string name) => Properties.TryGetValue(name, out var val) ? val : null;

            internal string StringValue(HazelcastProperty hazelcastProperty)
            {
                if (Properties.TryGetValue(hazelcastProperty.Name, out var val))
                {
                    return val;
                }
                var envVarValue = Environment.GetEnvironmentVariable(hazelcastProperty.Name);
                return envVarValue ?? hazelcastProperty.DefaultValue;
            }

            internal bool BoolValue(HazelcastProperty hazelcastProperty) => Convert.ToBoolean(StringValue(hazelcastProperty));
            internal int IntValue(HazelcastProperty hazelcastProperty) => int.Parse(StringValue(hazelcastProperty));
            internal long LongValue(HazelcastProperty hazelcastProperty) => long.Parse(StringValue(hazelcastProperty));
        }
    }
}