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
using System.IO;
using System.Xml;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Security;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Loads the <see cref="HazelcastConfiguration"/> using XML.
    /// </summary>
    public class XmlClientConfigBuilder : XmlConfigHelperBase
    {
        // TODO: this class should be massively refactored
        // + move to .NET Core configuration style
        // + move to JSON

        private readonly XmlDocument _document = new XmlDocument();

        private HazelcastConfiguration _hazelcastConfiguration;

        internal XmlClientConfigBuilder(TextReader reader)
        {
            try
            {
                _document.Load(reader);
                reader.Dispose();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse configuration file, giving up.");
            }
        }

        private XmlClientConfigBuilder(string configFile = null)
        {
            if (configFile == null)
            {
                configFile = Environment.GetEnvironmentVariable("hazelcast.client.config");
            }
            try
            {
                TextReader input = null;
                if (configFile != null)
                {
                    if (File.Exists(configFile))
                    {
                        input = File.OpenText(configFile);
                    }
                    else
                    {
                        throw new ConfigurationException($"Could not open file {configFile}.");
                    }
                }
                if (input == null)
                {
                    configFile = Directory.GetCurrentDirectory() + "\\hazelcast-client.xml";
                    if (File.Exists(configFile))
                    {
                        input = File.OpenText(configFile);
                    }
                    else
                    {
                        input = new StringReader(Resources.hazelcast_client_default);
                    }
                }

                try
                {
                    _document.Load(input);
                }
                finally
                {
                    input.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Exception while reading configuration.", e);
            }
        }

        /// <summary>
        /// Build a <see cref="HazelcastConfiguration"/> from an XML file
        /// </summary>
        /// <param name="configFile">hazelcast client XML config file</param>
        /// <returns>ClientConfig</returns>
        public static HazelcastConfiguration Build(string configFile = null)
        {
            return new XmlClientConfigBuilder(configFile).Init();
        }

        /// <summary>
        /// Build a <see cref="HazelcastConfiguration"/> from an XML file
        /// </summary>
        /// <param name="reader">Text reader to provide hazelcast client XML</param>
        /// <returns>ClientConfig</returns>
        public static HazelcastConfiguration Build(TextReader reader)
        {
            return new XmlClientConfigBuilder(reader).Init();
        }

        /// <summary>
        /// Creates a <see cref="HazelcastConfiguration"/> using the XML content
        /// </summary>
        /// <returns></returns>
        protected HazelcastConfiguration Init()
        {
            _hazelcastConfiguration = new HazelcastConfiguration();
            lock (_hazelcastConfiguration)
            {
                HandleConfig(_document.DocumentElement); //PARSE
            }
            return _hazelcastConfiguration;
        }

        private void HandleClusterMembers(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("address".Equals(CleanNodeName(child)))
                {
                    _hazelcastConfiguration.Networking.Addresses.Add(GetTextContent(child));
                }
            }
        }

        /// <exception cref="Exception"></exception>
        private void HandleConfig(XmlElement docElement)
        {
            foreach (XmlNode node in docElement.ChildNodes)
            {
                var nodeName = CleanNodeName(node.Name);
                switch (nodeName)
                {
                    case "security":
                        HandleSecurity(node);
                        break;
                    case "proxy-factories":
                        HandleProxyFactories(node);
                        break;
                    case "serialization":
                        HandleSerialization(node);
                        break;
                    case "cluster":
                        HandleCluster(node);
                        break;
                    case "listeners":
                        HandleListeners(node);
                        break;
                    case "network":
                        HandleNetwork(node);
                        break;
                    case "load-balancer":
                        HandleLoadBalancer(node);
                        break;
                    case "near-cache":
                        HandleNearCache(node);
                        break;
                    case "connection-strategy":
                        throw new NotImplementedException();
                }
            }
        }

        private void HandleCluster(XmlNode node)
        {
            var name = GetAttribute(node, "name");
            var password = GetAttribute(node, "password");

            if (name != null)
            {
                _hazelcastConfiguration.ClusterName = name;
            }

            if (password != null)
            {
                // _clientConfig.SetClusterPassword(password);
            }
        }

        /// <exception cref="Exception"></exception>
        private void HandleListeners(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("listener".Equals(CleanNodeName(child)))
                {
                    var className = GetTextContent(child);
                    _hazelcastConfiguration.AddClusterEventSubscriber(className);
                }
            }
        }

        private void HandleLoadBalancer(XmlNode node)
        {
            var type = GetAttribute(node, "type");
            if ("random".Equals(type))
            {
                var config = _hazelcastConfiguration.LoadBalancing;
                config.LoadBalancer = new RandomLoadBalancer();
            }
            else if ("round-robin".Equals(type))
            {
                var config = _hazelcastConfiguration.LoadBalancing;
                config.LoadBalancer = new RoundRobinLoadBalancer();
            }
        }

        private void HandleNearCache(XmlNode node)
        {
            var name = node.Attributes.GetNamedItem("name").Value;
            var nearCacheConfig = new NearCacheConfig(name);
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "max-size":
                        nearCacheConfig.SetMaxSize(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "time-to-live-seconds":
                        nearCacheConfig.SetTimeToLiveSeconds(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "max-idle-seconds":
                        nearCacheConfig.SetMaxIdleSeconds(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "eviction-policy":
                        nearCacheConfig.SetEvictionPolicy(GetTextContent(child));
                        break;
                    case "in-memory-format":
                        InMemoryFormat result;
                        Enum.TryParse(GetTextContent(child), true, out result);
                        nearCacheConfig.SetInMemoryFormat(result);
                        break;
                    case "invalidate-on-change":
                        nearCacheConfig.SetInvalidateOnChange(bool.Parse(GetTextContent(child)));
                        break;
                }
            }
            _hazelcastConfiguration.NearCache.Add(name, nearCacheConfig);
        }

        private void HandleNetwork(XmlNode node)
        {
            var clientNetworkConfig = _hazelcastConfiguration.Networking;
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "cluster-members":
                        HandleClusterMembers(child);
                        break;
                    case "smart-routing":
                        clientNetworkConfig.SmartRouting = bool.Parse(GetTextContent(child));
                        break;
                    case "redo-operation":
                        clientNetworkConfig.RedoOperation = bool.Parse(GetTextContent(child));
                        break;
                    case "connection-timeout":
                        clientNetworkConfig.ConnectionTimeoutMilliseconds = Convert.ToInt32(GetTextContent(child));
                        break;
                    case "socket-options":
                        HandleSocketOptions(child, clientNetworkConfig);
                        break;
                    case "ssl":
                        HandleSslConfiguration(child, clientNetworkConfig);
                        break;
                    case "hazelcast-cloud":
                        HandleCloudConfig(child, clientNetworkConfig);
                        break;
                    case "socket-interceptor":
                        HandleSocketInterceptorConfig(child, clientNetworkConfig);
                        break;
                }
            }
        }

        /// <exception cref="Exception"></exception>
        private void HandleProxyFactories(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child.Name);
                if ("proxy-factory".Equals(nodeName))
                {
                    HandleProxyFactory(child);
                }
            }
        }

        /// <exception cref="Exception"></exception>
        private void HandleProxyFactory(XmlNode node)
        {
            var service = GetAttribute(node, "service");
            var className = GetAttribute(node, "class-name");
            // var proxyFactoryConfig = new ProxyFactoryConfig(className, service);
            // _clientConfig.AddProxyFactoryConfig(proxyFactoryConfig);
        }

        /// <exception cref="Exception"></exception>
        private void HandleSecurity(XmlNode node)
        {
            var clientSecurityConfig = _hazelcastConfiguration.Security;
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("credentials".Equals(nodeName))
                {
                    // TODO: obsolete that method, it is not supported
                }
                else if ("credentials-factory".Equals(nodeName))
                {
                    HandleCredentialsFactory(child, clientSecurityConfig);
                }
            }
        }

        private void HandleCredentialsFactory(XmlNode node, SecurityConfiguration securityConfiguration)
        {
            var classname = GetAttribute(node, "class-name");
            var factory = Services.CreateInstance<ICredentialsFactory>(classname);

            foreach (XmlNode child in node.ChildNodes) {
                var nodeName = CleanNodeName(child.Name);
                if ("properties".Equals(nodeName))
                {
                    var props = new Dictionary<string, string>();
                    FillProperties(child, props);
                    factory.Initialize(props);
                    break;
                }
            }

            securityConfiguration.CredentialsFactory = factory;
        }

        private void HandleSerialization(XmlNode node)
        {
            ParseSerialization(_hazelcastConfiguration.Serialization, node);
        }

        private void HandleSocketInterceptorConfig(XmlNode node, NetworkingConfiguration networkingConfiguration)
        {
            ParseSocketInterceptorConfiguration(networkingConfiguration.SocketInterceptor, node);
        }

        private void HandleSocketOptions(XmlNode node, NetworkingConfiguration networkingConfiguration)
        {
            var socketOptions = networkingConfiguration.SocketOptions;
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "tcp-no-delay":
                        socketOptions.TcpNoDelay = bool.Parse(GetTextContent(child));
                        break;
                    case "keep-alive":
                        socketOptions.KeepAlive = bool.Parse(GetTextContent(child));
                        break;
                    case "reuse-address":
                        socketOptions.ReuseAddress = bool.Parse(GetTextContent(child));
                        break;
                    case "linger-seconds":
                        socketOptions.LingerSeconds = Convert.ToInt32(GetTextContent(child));
                        break;
                    case "buffer-size":
                        socketOptions.BufferSize = Convert.ToInt32(GetTextContent(child));
                        break;
                }
            }
        }
        private void HandleSslConfiguration(XmlNode node, NetworkingConfiguration networkingConfiguration)
        {
            ParseSslConfiguration(networkingConfiguration.SslConfiguration, node);
        }

        private void HandleCloudConfig(XmlNode node, NetworkingConfiguration networkingConfiguration) {
            var cloudConfig = networkingConfiguration.CloudConfiguration;

            var enabledNode = node.Attributes.GetNamedItem("enabled");
            var enabled = enabledNode != null && CheckTrue(GetTextContent(enabledNode).Trim());
            cloudConfig.IsEnabled = enabled;
            foreach (XmlNode child in node.ChildNodes) {
                var nodeName = CleanNodeName(child);
                if ("discovery-token".Equals(nodeName)) {
                    cloudConfig.DiscoveryToken = GetTextContent(child);
                }
            }
        }

    }
}