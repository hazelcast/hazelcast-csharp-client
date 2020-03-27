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
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    internal static class XmlParserHelper
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(XmlParserHelper));

        public static void Build(this Configuration configuration, XmlDocument document)
        {
            var rootNode = document.DocumentElement;
            CheckRootElement(rootNode);
            var occurrenceSet = new HashSet<string>();
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    //ignore not elements
                    continue;
                }
                var nodeName = CleanNodeName(node.Name);
                if (occurrenceSet.Contains(nodeName))
                {
                    throw new InvalidConfigurationException("Duplicate '" + nodeName + "' definition found in the configuration");
                }
                configuration.HandleNode(node, nodeName);
                if (!CanOccurMultipleTimes(nodeName))
                {
                    occurrenceSet.Add(nodeName);
                }
            }
        }

        private static void HandleNode(this Configuration configuration, XmlNode node, string nodeName)
        {
            switch (nodeName)
            {
                case "security":
                    configuration.SecurityConfig.HandleSecurity(node);
                    break;
                case "properties":
                    configuration.Properties.FillProperties(node);
                    break;
                case "serialization":
                    configuration.SerializationConfig.HandleSerialization(node);
                    break;
                case "listeners":
                    configuration.ListenerConfigs.HandleListeners(node);
                    break;
                case "network":
                    configuration.NetworkConfig.HandleNetwork(node);
                    break;
                case "load-balancer":
                    configuration.HandleLoadBalancer(node);
                    break;
                case "near-cache":
                    var name = GetAttribute(node, "name");
                    configuration.ConfigureNearCache(name, nearCacheConfig =>
                    {
                        nearCacheConfig.HandleNearCache(node);
                    });
                    break;
                case "backup-ack-to-client-enabled":
                    configuration.BackupAckToClientEnabled = Convert.ToBoolean(GetTextContent(node));
                    break;
                case "instance-name":
                    configuration.InstanceName = GetTextContent(node);
                    break;
                case "client-labels":
                    configuration.Labels.HandleLabels(node);
                    break;
                case "cluster-name":
                    configuration.ClusterName = GetTextContent(node);
                    break;
                case "connection-strategy":
                    configuration.ConfigureConnectionStrategy(cs =>
                    {
                        cs.HandleConnectionStrategy(node);
                    });
                    break;
                //not supported xsd sections
                default:
                    throw new InvalidConfigurationException($"Undefined configuration name:{nodeName}.");
            }
        }

        private static void HandleSecurity(this SecurityConfig securityConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("username-password".Equals(nodeName))
                {
                    securityConfig.ConfigureUsernamePasswordIdentity(GetAttribute(child, "username"),
                        GetAttribute(child, "password"));
                }
                else if ("token".Equals(nodeName))
                {
                    var encodingLabel = GetAttribute(node, "encoding");
                    var encodedToken = GetTextContent(node);

                    TokenEncoding tokenEncoding = Enum.TryParse(encodingLabel, true, out tokenEncoding)
                        ? tokenEncoding
                        : TokenEncoding.None;
                    securityConfig.ConfigureTokenIdentity(encodedToken, tokenEncoding);
                }
                else if ("credentials-factory".Equals(nodeName))
                {
                    securityConfig.HandleCredentialsFactory(child);
                }
            }
        }

        private static void HandleCredentialsFactory(this SecurityConfig securityConfig, XmlNode node)
        {
            var typeName = GetAttribute(node, "class-name");
            var prop = new Dictionary<string, string>();
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child.Name);
                if ("properties".Equals(nodeName))
                {
                    prop.FillProperties(child);
                    break;
                }
            }
            securityConfig.ConfigureCredentialsFactory(typeName, prop);
        }

        private static void HandleSerialization(this SerializationConfig serializationConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                switch (name)
                {
                    case "portable-version":
                        serializationConfig.PortableVersion= GetIntegerValue(name, GetTextContent(child), 0);
                        break;
                    case "check-class-def-errors":
                        serializationConfig.CheckClassDefErrors = CheckTrue(GetTextContent(child));
                        break;
                    case "use-native-byte-order":
                        serializationConfig.UseNativeByteOrder = CheckTrue(GetTextContent(child));
                        break;
                    case "byte-order":
                        var bigEndian = GetTextContent(child);
                        var byteOrder = ByteOrder.GetByteOrder(bigEndian);
                        serializationConfig.ByteOrder = byteOrder;
                        break;
                    case "data-serializable-factories":
                        serializationConfig.FillDataSerializableFactories(child);
                        break;
                    case "portable-factories":
                        serializationConfig.FillPortableFactories(child);
                        break;
                    case "serializers":
                        serializationConfig.FillSerializers(child);
                        break;
                    default:
                        throw new InvalidConfigurationException($"Not supported xml tag {name}");
                }
            }
        }

        private static void HandleListeners(this IList<ListenerConfig> listenerConfigs, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("listener".Equals(CleanNodeName(child)))
                {
                    var typeName = GetTextContent(child);
                    listenerConfigs.Add(new ListenerConfig(typeName));
                }
            }
        }

        private static void HandleNetwork(this NetworkConfig networkConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "cluster-members":
                        networkConfig.Addresses.HandleClusterMembers(child);
                        break;
                    case "smart-routing":
                        networkConfig.SmartRouting = Convert.ToBoolean(GetTextContent(child));
                        break;
                    case "redo-operation":
                        networkConfig.RedoOperation = Convert.ToBoolean(GetTextContent(child));
                        break;
                    case "connection-timeout":
                        networkConfig.ConnectionTimeout = Convert.ToInt32(GetTextContent(child));
                        break;
                    case "socket-options":
                        networkConfig.SocketOptions.HandleSocketOptions(child);
                        break;
                    case "ssl":
                        networkConfig.SslConfig.HandleSSLConfig(child);
                        break;
                    case "outbound-ports":
                        networkConfig.OutboundPorts.HandleOutboundPorts(child);
                        break;
                    case "hazelcast-cloud":
                        networkConfig.HazelcastCloudConfig.HandleHazelcastCloud(child);
                        break;
                    case "socket-interceptor":
                    case "discovery-strategies":
                    case "icmp-ping":
                        throw new InvalidConfigurationException($"Xml tag:{nodeName} is not supported.");
                }
            }
        }

        private static void HandleLoadBalancer(this Configuration configuration, XmlNode node)
        {
            var type = GetAttribute(node, "type");
            if ("random".Equals(type))
            {
                configuration.LoadBalancer = new RandomLB();
            }
            else if ("round-robin".Equals(type))
            {
                configuration.LoadBalancer = new RoundRobinLB();
            }
        }

        private static void HandleNearCache(this NearCacheConfig nearCacheConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                var textContent = GetTextContent(child);
                switch (nodeName)
                {
                    case "max-size":
                        nearCacheConfig.MaxSize = Convert.ToInt32(textContent);
                        break;
                    case "time-to-live-seconds":
                        nearCacheConfig.TimeToLiveSeconds = Convert.ToInt32(textContent);
                        break;
                    case "max-idle-seconds":
                        nearCacheConfig.MaxIdleSeconds = Convert.ToInt32(textContent);
                        break;
                    case "in-memory-format":
                        Enum.TryParse(textContent, true, out InMemoryFormat result);
                        nearCacheConfig.InMemoryFormat = result;
                        break;
                    case "serialize-keys":
                        nearCacheConfig.SerializeKeys = Convert.ToBoolean(textContent);
                        break;
                    case "invalidate-on-change":
                        nearCacheConfig.InvalidateOnChange = Convert.ToBoolean(textContent);
                        break;
                    case "eviction-policy":
                        nearCacheConfig.EvictionPolicy = (EvictionPolicy) Enum.Parse(typeof(EvictionPolicy), textContent, true);
                        break;
                    default:
                        throw new InvalidConfigurationException($"Xml tag:{nodeName} is not supported.");
                }
            }
        }

        private static void HandleLabels(this ISet<string> labels, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                labels.Add(GetTextContent(child));
            }
        }

        private static void HandleConnectionStrategy(this ConnectionStrategyConfig connectionStrategyConfig, XmlNode node)
        {
            
            connectionStrategyConfig.AsyncStart = Convert.ToBoolean(GetAttribute(node, "async-start").Trim());
            var attrValue = GetAttribute(node, "reconnect-mode");
            if (attrValue != null)
            {
                connectionStrategyConfig.ReconnectMode = (ReconnectMode) Enum.Parse(typeof(ReconnectMode), attrValue.Trim(), true);
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("connection-retry".Equals(nodeName))
                {
                    connectionStrategyConfig.ConnectionRetryConfig.HandleConnectionRetry(child);
                }
            }
        }

        private static void HandleConnectionRetry(this ConnectionRetryConfig connectionRetryConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                var textContent = GetTextContent(child).Trim();
                switch (nodeName)
                {
                    case "initial-backoff-millis":
                        connectionRetryConfig.InitialBackoffMillis = Convert.ToInt32(textContent);
                        break;
                    case "max-backoff-millis":
                        connectionRetryConfig.MaxBackoffMillis = Convert.ToInt32(textContent);
                        break;
                    case "multiplier":
                        connectionRetryConfig.Multiplier = Convert.ToDouble(textContent);
                        break;
                    case "jitter":
                        connectionRetryConfig.Jitter = Convert.ToDouble(textContent);
                        break;
                    case "cluster-connect-timeout-millis":
                        connectionRetryConfig.ClusterConnectTimeoutMillis = Convert.ToInt64(textContent);
                        break;
                }
            }
        }

        private static void FillDataSerializableFactories(this SerializationConfig serializationConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                if ("data-serializable-factory".Equals(name))
                {
                    var value = GetTextContent(child);
                    var factoryIdNode = child?.Attributes?.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException("'factory-id' attribute of 'data-serializable-factory' is required!");
                    }
                    var factoryId = Convert.ToInt32(GetTextContent(factoryIdNode));
                    serializationConfig.DataSerializableFactoryClasses.Add(factoryId, value);
                }
            }
        }

        private static void FillPortableFactories(this SerializationConfig serializationConfig, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = CleanNodeName(child);
                if ("portable-factory".Equals(name))
                {
                    var value = GetTextContent(child);
                    var factoryIdNode = child?.Attributes?.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException("'factory-id' attribute of 'portable-factory' is required!");
                    }
                    var factoryId = Convert.ToInt32(GetTextContent(factoryIdNode));
                    serializationConfig.PortableFactoryClasses.Add(factoryId, value);
                }
            }
        }

        private static void FillSerializers(this SerializationConfig serializationConfig, XmlNode node)
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
                    serializationConfig.SerializerConfigs.Add(serializerConfig);
                }
                else
                {
                    if ("global-serializer".Equals(name))
                    {
                        serializationConfig.ConfigureGlobalSerializer(gs => { gs.TypeName = value; });
                    }
                }
            }
        }

        private static void HandleClusterMembers(this IList<string> list, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("address".Equals(CleanNodeName(child)))
                {
                    list.Add(GetTextContent(child));
                }
            }
        }

        private static void HandleSocketOptions(this SocketOptions socketOptions, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                var textContent = GetTextContent(child);
                switch (nodeName)
                {
                    case "tcp-no-delay":
                        socketOptions.TcpNoDelay = Convert.ToBoolean(textContent);
                        break;
                    case "keep-alive":
                        socketOptions.KeepAlive = Convert.ToBoolean(textContent);
                        break;
                    case "reuse-address":
                        socketOptions.ReuseAddress = Convert.ToBoolean(textContent);
                        break;
                    case "linger-seconds":
                        socketOptions.LingerSeconds = Convert.ToInt32(textContent);
                        break;
                    case "buffer-size":
                        socketOptions.BufferSize = Convert.ToInt32(textContent);
                        break;
                }
            }
        }

        private static void HandleSSLConfig(this SSLConfig sslConfig, XmlNode node)
        {
            var enabledNode = node?.Attributes?.GetNamedItem("enabled");
            sslConfig.Enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            foreach (XmlNode n in node.ChildNodes)
            {
                var nodeName = CleanNodeName(n.Name);
                if ("properties".Equals(nodeName))
                {
                    var props = new Dictionary<string, string>();
                    props.FillProperties(n);
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
                        sslConfig.CertificateFilePath = certificateFilePath;
                    }
                    if (props.TryGetValue(HazelcastProperties.CertificatePassword, out var certificatePassword))
                    {
                        sslConfig.CertificatePassword = certificatePassword;
                    }
                    if (props.TryGetValue(HazelcastProperties.SslProtocol, out var sslProtocol))
                    {
                        sslConfig.SslProtocol = (SslProtocols) Enum.Parse(typeof(SslProtocols), sslProtocol, true);
                    }
                }
            }
        }

        private static void HandleHazelcastCloud(this HazelcastCloudConfig cloudConfig, XmlNode node)
        {
            var enabledNode = node.Attributes.GetNamedItem("enabled");
            cloudConfig.Enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("discovery-token".Equals(nodeName))
                {
                    cloudConfig.DiscoveryToken = GetTextContent(child);
                }
            }
        }

        private static void HandleOutboundPorts(this ICollection<string> ports, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("ports".Equals(nodeName))
                {
                    var value = GetTextContent(child);
                    ports.Add(value);
                }
            }
        }

        //Utils
        private static string CleanNodeName(XmlNode node)
        {
            return CleanNodeName(node.Name);
        }

        private static string CleanNodeName(string nodeName)
        {
            var name = nodeName;
            if (name != null)
            {
                name = Regex.Replace(nodeName, "\\w+:", string.Empty).ToLower();
            }

            return name;
        }

        private static bool CheckTrue(string value)
        {
            return "true".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                   "yes".Equals(value, StringComparison.OrdinalIgnoreCase) ||
                   "on".Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetAttribute(XmlNode node, string attName)
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

        private static int GetIntegerValue(string parameterName, string value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception e)
            {
                Logger.Info(parameterName + " parameter value, [" + value + "], is not a proper integer. Default value, [" +
                            defaultValue + "], will be used!");
                Logger.Warning(e);
                return defaultValue;
            }
        }

        private static string GetTextContent(XmlNode node)
        {
            if (node != null)
            {
                return node.InnerText.Trim();
            }

            return string.Empty;
        }

        private static void FillProperties(this IDictionary<string, string> properties, XmlNode node)
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

        private static void CheckRootElement(XmlNode root)
        {
            const string rootName = "hazelcast-client";
            if (root == null || !rootName.Equals(root.Name))
            {
                throw new InvalidConfigurationException(
                    $"Invalid root element in xml configuration! Expected: <{rootName}>, Actual: <{root?.Name}>.");
            }
        }

        private static readonly IDictionary<string, bool> ConfigSectionNames = new Dictionary<string, bool>
        {
            {"hazelcast-client", false},
            {"import", true},
            {"security", false},
            {"proxy-factories", false},
            {"properties", false},
            {"serialization", false},
            {"native-memory", false},
            {"listeners", false},
            {"network", false},
            {"load-balancer", false},
            {"near-cache", true},
            {"query-caches", false},
            {"backup-ack-to-client-enabled", false},
            {"instance-name", false},
            {"connection-strategy", false},
            {"user-code-deployment", false},
            {"flake-id-generator", true},
            {"reliable-topic", true},
            {"client-labels", false},
            {"cluster-name", false},
            {"metrics", false}
        };

        private static bool CanOccurMultipleTimes(string enumName)
        {
            foreach (var kvp in ConfigSectionNames)
            {
                if (kvp.Key.Equals(enumName))
                {
                    return kvp.Value;
                }
            }
            return true;
        }
    }
}