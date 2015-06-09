using System;
using System.IO;
using System.Xml;
using Hazelcast.Logging;
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    public class XmlClientConfigBuilder : AbstractXmlConfigHelper
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (XmlClientConfigBuilder));

        private ClientConfig clientConfig;

        private readonly XmlDocument document = new XmlDocument();

        internal XmlClientConfigBuilder(TextReader reader)
        {
            try
            {
                document.Load(reader);
                reader.Dispose();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse configuration file, giving up.");
            }
        }

        private XmlClientConfigBuilder(string configFile=null)
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
                        string msg = "Config file at '" + configFile + "' doesn't exist.";
                        msg +=
                            "\nHazelcast will try to use the hazelcast-client.xml config file in the working directory.";
                        logger.Warning(msg);
                    }
                }
                if (input == null)
                {
                    configFile = Directory.GetCurrentDirectory()+"\\hazelcast-client.xml";
                    if (File.Exists(configFile))
                    {
                        input = File.OpenText(configFile);
                        logger.Info("Using configuration file at working dir.");
                    }
                    else
                    {
                         input = new StringReader(Properties.Resources.hazelcast_client_default);
                        logger.Info("Using Default configuration file");
                    }
                }
                try
                {
                    document.Load(input);
                    input.Dispose();
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Could not parse configuration file, giving up.");
                }
            }
            catch (Exception e)
            {
                logger.Severe("Error while creating configuration:" + e.Message, e);
            }
        }
        
        public static ClientConfig Build(string configFile = null)
        {
            return new XmlClientConfigBuilder(configFile).Init();
        }

        public static ClientConfig Build(TextReader reader)
        {
            return new XmlClientConfigBuilder(reader).Init();
        }

        protected ClientConfig Init()
        {
            this.clientConfig = new ClientConfig();
            try
            {
                lock (this.clientConfig)
                {
                    HandleConfig(document.DocumentElement);//PARSE
                }
                return clientConfig;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleConfig(XmlElement docElement)
        {
            foreach (XmlNode node in docElement.ChildNodes)
            {
                string nodeName = CleanNodeName(node.Name);
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
                    case "license-key":
                        clientConfig.SetLicenseKey(GetTextContent(node));
                        break;
                }
            }
        }

        private void HandleNearCache(XmlNode node)
        {
            string name = node.Attributes.GetNamedItem("name").Value;
            var nearCacheConfig = new NearCacheConfig();
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child);
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
                        nearCacheConfig.SetInvalidateOnChange(Boolean.Parse(GetTextContent(child)));
                        break;
                }
            }
            clientConfig.AddNearCacheConfig(name, nearCacheConfig);
        }

        private void HandleLoadBalancer(XmlNode node)
        {
            string type = GetAttribute(node, "type");
            if ("random".Equals(type))
            {
                clientConfig.SetLoadBalancer(new RandomLB());
            }
            else if ("round-robin".Equals(type))
            {
                clientConfig.SetLoadBalancer(new RoundRobinLB());
            }
        }

        private void HandleNetwork(XmlNode node)
        {
            var clientNetworkConfig = clientConfig.GetNetworkConfig();
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "cluster-members":
                        HandleClusterMembers(child);
                        break;

                    case "smart-routing":
                        clientNetworkConfig.SetSmartRouting(Boolean.Parse(GetTextContent(child)));
                        break;
                    case "redo-operation":
                        clientNetworkConfig.SetRedoOperation(Boolean.Parse(GetTextContent(child)));
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
                    case "socket-interceptor":
                        HandleSocketInterceptorConfig(node);
                        break;
                }
            }
        }

        private void HandleSocketOptions(XmlNode node, ClientNetworkConfig clientNetworkConfig)
        {
            SocketOptions socketOptions = clientNetworkConfig.GetSocketOptions();
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child);
                switch (nodeName)
                {
                    case "tcp-no-delay":
                        socketOptions.SetTcpNoDelay(Boolean.Parse(GetTextContent(child)));
                        break;
                    case "keep-alive":
                        socketOptions.SetKeepAlive(Boolean.Parse(GetTextContent(child)));
                        break;
                    case "reuse-address":
                        socketOptions.SetReuseAddress(Boolean.Parse(GetTextContent(child)));
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

        private void HandleClusterMembers(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("address".Equals(CleanNodeName(child)))
                {
                    clientConfig.GetNetworkConfig().AddAddress(GetTextContent(child));
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
                    string className = GetTextContent(child);
                    clientConfig.AddListenerConfig(new ListenerConfig(className));
                }
            }
        }

        private void HandleGroup(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                string value = GetTextContent(n).Trim();
                string nodeName = CleanNodeName(n.Name);
                if ("name".Equals(nodeName))
                {
                    clientConfig.GetGroupConfig().SetName(value);
                }
                else
                {
                    if ("password".Equals(nodeName))
                    {
                        clientConfig.GetGroupConfig().SetPassword(value);
                    }
                }
            }
        }

        private void HandleSerialization(XmlNode node)
        {
            SerializationConfig serializationConfig = ParseSerialization(node);
            clientConfig.SetSerializationConfig(serializationConfig);
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleProxyFactories(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child.Name);
                if ("proxy-factory".Equals(nodeName))
                {
                    HandleProxyFactory(child);
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleProxyFactory(XmlNode node)
        {
            string service = GetAttribute(node, "service");
            string className = GetAttribute(node, "class-name");
            var proxyFactoryConfig = new ProxyFactoryConfig(className, service);
            clientConfig.AddProxyFactoryConfig(proxyFactoryConfig);
        }

        private void HandleSocketInterceptorConfig(XmlNode node)
        {
            SocketInterceptorConfig socketInterceptorConfig = ParseSocketInterceptorConfig(node);
            clientConfig.GetNetworkConfig().SetSocketInterceptorConfig(socketInterceptorConfig);
        }

        /// <exception cref="System.Exception"></exception>
        private void HandleSecurity(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child.Name);
                if ("login-credentials".Equals(nodeName))
                {
                    HandleLoginCredentials(child);
                }
            }
        }

        private void HandleLoginCredentials(XmlNode node)
        {
            var credentials = new UsernamePasswordCredentials();
            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = CleanNodeName(child.Name);
                if ("username".Equals(nodeName))
                {
                    credentials.SetUsername(GetTextContent(child));
                }
                else
                {
                    if ("password".Equals(nodeName))
                    {
                        credentials.SetPassword(GetTextContent(child));
                    }
                }
            }
            clientConfig.SetCredentials(credentials);
        }
    }
}