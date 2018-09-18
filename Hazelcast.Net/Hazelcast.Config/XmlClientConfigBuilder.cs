// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Xml;
using Hazelcast.Client.Properties;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>
    /// Loads the <see cref="ClientConfig"/> using XML.
    /// </summary>
    public class XmlClientConfigBuilder : AbstractXmlConfigHelper
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (XmlClientConfigBuilder));

        private readonly XmlDocument _document = new XmlDocument();

        private ClientConfig _clientConfig;

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
                        var msg = "Config file at '" + configFile + "' doesn't exist.";
                        msg +=
                            "\nHazelcast will try to use the hazelcast-client.xml config file in the working directory.";
                        Logger.Warning(msg);
                    }
                }
                if (input == null)
                {
                    configFile = Directory.GetCurrentDirectory() + "\\hazelcast-client.xml";
                    if (File.Exists(configFile))
                    {
                        input = File.OpenText(configFile);
                        Logger.Info("Using configuration file at working dir.");
                    }
                    else
                    {
                        input = new StringReader(Resources.hazelcast_client_default);
                        Logger.Info("Using Default configuration file");
                    }
                }
                try
                {
                    _document.Load(input);
                    input.Dispose();
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Could not parse configuration file, giving up.");
                }
            }
            catch (Exception e)
            {
                Logger.Severe("Error while creating configuration:" + e.Message, e);
            }
        }

        /// <summary>
        /// Build a <see cref="ClientConfig"/> from an XML file
        /// </summary>
        /// <param name="configFile">hazelcast client XML config file</param>
        /// <returns>ClientConfig</returns>
        public static ClientConfig Build(string configFile = null)
        {
            return new XmlClientConfigBuilder(configFile).Init();
        }

        /// <summary>
        /// Build a <see cref="ClientConfig"/> from an XML file
        /// </summary>
        /// <param name="reader">Text reader to provide hazelcast client XML</param>
        /// <returns>ClientConfig</returns>
        public static ClientConfig Build(TextReader reader)
        {
            return new XmlClientConfigBuilder(reader).Init();
        }

        /// <summary>
        /// Creates a <see cref="ClientConfig"/> using the XML content 
        /// </summary>
        /// <returns></returns>
        protected ClientConfig Init()
        {
            _clientConfig = new ClientConfig();
            try
            {
                lock (_clientConfig)
                {
                    HandleConfig(_document.DocumentElement); //PARSE
                }
                return _clientConfig;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        private void HandleClusterMembers(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("address".Equals(CleanNodeName(child)))
                {
                    _clientConfig.GetNetworkConfig().AddAddress(GetTextContent(child));
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
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
                    case "group":
                        HandleGroup(node);
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
                }
            }
        }

        private void HandleGroup(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                var value = GetTextContent(n).Trim();
                var nodeName = CleanNodeName(n.Name);
                if ("name".Equals(nodeName))
                {
                    _clientConfig.GetGroupConfig().SetName(value);
                }
                else
                {
                    if ("password".Equals(nodeName))
                    {
                        _clientConfig.GetGroupConfig().SetPassword(value);
                    }
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleListeners(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("listener".Equals(CleanNodeName(child)))
                {
                    var className = GetTextContent(child);
                    _clientConfig.AddListenerConfig(new ListenerConfig(className));
                }
            }
        }

        private void HandleLoadBalancer(XmlNode node)
        {
            var type = GetAttribute(node, "type");
            if ("random".Equals(type))
            {
                _clientConfig.SetLoadBalancer(new RandomLB());
            }
            else if ("round-robin".Equals(type))
            {
                _clientConfig.SetLoadBalancer(new RoundRobinLB());
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
            _clientConfig.AddNearCacheConfig(name, nearCacheConfig);
        }

        private void HandleNetwork(XmlNode node)
        {
            var clientNetworkConfig = _clientConfig.GetNetworkConfig();
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "cluster-members":
                        HandleClusterMembers(child);
                        break;

                    case "smart-routing":
                        clientNetworkConfig.SetSmartRouting(bool.Parse(GetTextContent(child)));
                        break;
                    case "redo-operation":
                        clientNetworkConfig.SetRedoOperation(bool.Parse(GetTextContent(child)));
                        break;
                    case "connection-timeout":
                        clientNetworkConfig.SetConnectionTimeout(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "connection-attempt-period":
                        clientNetworkConfig.SetConnectionAttemptPeriod(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "connection-attempt-limit":
                        clientNetworkConfig.SetConnectionAttemptLimit(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "socket-options":
                        HandleSocketOptions(child, clientNetworkConfig);
                        break;
                    case "ssl":
                        HandleSSLConfig(child, clientNetworkConfig);
                        break;
                    case "socket-interceptor":
                        HandleSocketInterceptorConfig(child, clientNetworkConfig);
                        break;
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
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

        /// <exception cref="System.Exception"></exception>
        private void HandleProxyFactory(XmlNode node)
        {
            var service = GetAttribute(node, "service");
            var className = GetAttribute(node, "class-name");
            var proxyFactoryConfig = new ProxyFactoryConfig(className, service);
            _clientConfig.AddProxyFactoryConfig(proxyFactoryConfig);
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleSecurity(XmlNode node)
        {
            var clientSecurityConfig = new ClientSecurityConfig();
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                if ("credentials".Equals(nodeName))
                {
                    var className = GetTextContent(child);
                    clientSecurityConfig.SetCredentialsClassName(className);
                }
                else if ("credentials-factory".Equals(nodeName))
                {
                    HandleCredentialsFactory(child, clientSecurityConfig);
                }
            }
            _clientConfig.SetSecurityConfig(clientSecurityConfig);
        }

        private void HandleCredentialsFactory(XmlNode node, ClientSecurityConfig clientSecurityConfig)
        {
            var className = GetAttribute(node, "class-name");
            var credentialsFactoryConfig = new CredentialsFactoryConfig(className);
            clientSecurityConfig.SetCredentialsFactoryConfig(credentialsFactoryConfig);
            foreach (XmlNode child in node.ChildNodes) {
                var nodeName = CleanNodeName(child.Name);
                if ("properties".Equals(nodeName))
                {
                    FillProperties(child, credentialsFactoryConfig.GetProperties());
                    break;
                }
            }
        }

        private void HandleSerialization(XmlNode node)
        {
            var serializationConfig = ParseSerialization(node);
            _clientConfig.SetSerializationConfig(serializationConfig);
        }

        private void HandleSocketInterceptorConfig(XmlNode node, ClientNetworkConfig clientNetworkConfig)
        {
            var socketInterceptorConfig = ParseSocketInterceptorConfig(node);
            clientNetworkConfig.SetSocketInterceptorConfig(socketInterceptorConfig);
        }

        private void HandleSocketOptions(XmlNode node, ClientNetworkConfig clientNetworkConfig)
        {
            var socketOptions = clientNetworkConfig.GetSocketOptions();
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "tcp-no-delay":
                        socketOptions.SetTcpNoDelay(bool.Parse(GetTextContent(child)));
                        break;
                    case "keep-alive":
                        socketOptions.SetKeepAlive(bool.Parse(GetTextContent(child)));
                        break;
                    case "reuse-address":
                        socketOptions.SetReuseAddress(bool.Parse(GetTextContent(child)));
                        break;
                    case "linger-seconds":
                        socketOptions.SetLingerSeconds(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "timeout":
                        socketOptions.SetTimeout(Convert.ToInt32(GetTextContent(child)));
                        break;
                    case "buffer-size":
                        socketOptions.SetBufferSize(Convert.ToInt32(GetTextContent(child)));
                        break;
                }
            }
        }
        private void HandleSSLConfig(XmlNode node, ClientNetworkConfig clientNetworkConfig)
        {
            var sslConfig = ParseSSLConfig(node);
            clientNetworkConfig.SetSSLConfig(sslConfig);
        }
    }
}