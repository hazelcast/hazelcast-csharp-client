using System;
using System.Xml;
using Hazelcast.Logging;

//using Hazelcast.Net.Ext;
//using Hazelcast.Security;
//using Hazelcast.Util;

namespace Hazelcast.Config
{
    public class XmlClientConfigBuilder : AbstractXmlConfigHelper
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (XmlClientConfigBuilder));

        private ClientConfig clientConfig;

        private XmlReader reader;

        /// <exception cref="System.IO.IOException"></exception>
        public XmlClientConfigBuilder(string configFileUri)
        {
            reader = XmlReader.Create(configFileUri);
            if (reader == null)
            {
                throw new ArgumentNullException("configFileUri", "Could not load " + configFileUri);
            }
        }


        public XmlClientConfigBuilder()
        {
            //string configFile = Runtime.GetProperty("hazelcast.client.config");
            //try
            //{
            //    FilePath configurationFile = null;
            //    if (configFile != null)
            //    {
            //        configurationFile = new FilePath(configFile);
            //        logger.Info("Using configuration file at " + configurationFile.GetAbsolutePath());
            //        if (!configurationFile.Exists())
            //        {
            //            string msg = "Config file at '" + configurationFile.GetAbsolutePath() + "' doesn't exist.";
            //            msg += "\nHazelcast will try to use the hazelcast-client.xml config file in the working directory.";
            //            logger.Warning(msg);
            //            configurationFile = null;
            //        }
            //    }
            //    if (configurationFile == null)
            //    {
            //        configFile = "hazelcast-client-default.xml";
            //        configurationFile = new FilePath("hazelcast-client-default.xml");
            //        if (!configurationFile.Exists())
            //        {
            //            configurationFile = null;
            //        }
            //    }
            //    Uri configurationUrl;
            //    if (configurationFile != null)
            //    {
            //        logger.Info("Using configuration file at " + configurationFile.GetAbsolutePath());
            //        try
            //        {
            //            input = new FileInputStream(configurationFile);
            //            configurationUrl = configurationFile.ToURI().ToURL();
            //        }
            //        catch (Exception e)
            //        {
            //            string msg = "Having problem reading config file at '" + configFile + "'.";
            //            msg += "\nException message: " + e.Message;
            //            msg += "\nHazelcast will try to use the hazelcast-client.xml config file in classpath.";
            //            logger.Warning(msg);
            //            input = null;
            //        }
            //    }
            //    if (input == null)
            //    {
            //        logger.Info("Looking for hazelcast-client.xml config file in classpath.");
            //        //TODO CONFIG NASIL LOAD OLACAK
            //        configurationUrl = null;
            //        //ClientConfig.class.getClassLoader().getResource("hazelcast-client-default.xml");
            //        if (configurationUrl == null)
            //        {
            //            throw new InvalidOperationException("Cannot find hazelcast-client.xml in classpath, giving up.");
            //        }
            //        logger.Info("Using configuration file " + configurationUrl.GetFile() + " in the classpath.");
            //        input = configurationUrl.OpenStream();
            //        if (input == null)
            //        {
            //            throw new InvalidOperationException("Cannot read configuration file, giving up.");
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.Severe("Error while creating configuration:" + e.Message, e);
            //}
        }

        public virtual ClientConfig Build()
        {
            var clientConfig = new ClientConfig();

            //TODO NOT IMPLEMENTED
            return clientConfig;
            //try
            //{
            //    Parse(clientConfig);
            //    return clientConfig;
            //}
            //catch (Exception e)
            //{
            //    throw ExceptionUtil.Rethrow(e);
            //}
        }

        ///// <exception cref="System.Exception"></exception>
        //private void Parse(ClientConfig clientConfig)
        //{
        //    this.clientConfig = clientConfig;
        //    DocumentBuilder builder = DocumentBuilderFactory.NewInstance().NewDocumentBuilder();
        //    Document doc;
        //    try
        //    {
        //        doc = builder.Parse(input);
        //    }
        //    catch (Exception)
        //    {
        //        throw new InvalidOperationException("Could not parse configuration file, giving up.");
        //    }
        //    Element element = doc.GetDocumentElement();
        //    try
        //    {
        //        element.GetTextContent();
        //    }
        //    catch
        //    {
        //        domLevel3 = false;
        //    }
        //    HandleConfig(element);
        //}

        ///// <exception cref="System.Exception"></exception>
        //private void HandleConfig(Element docElement)
        //{
        //    foreach (Node node in new AbstractXmlConfigHelper.IterableNodeList(docElement.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(node.GetNodeName());
        //        if ("security".Equals(nodeName))
        //        {
        //            HandleSecurity(node);
        //        }
        //        else
        //        {
        //            if ("proxy-factories".Equals(nodeName))
        //            {
        //                HandleProxyFactories(node);
        //            }
        //            else
        //            {
        //                if ("serialization".Equals(nodeName))
        //                {
        //                    HandleSerialization(node);
        //                }
        //                else
        //                {
        //                    if ("group".Equals(nodeName))
        //                    {
        //                        HandleGroup(node);
        //                    }
        //                    else
        //                    {
        //                        if ("listeners".Equals(nodeName))
        //                        {
        //                            HandleListeners(node);
        //                        }
        //                        else
        //                        {
        //                            if ("network".Equals(nodeName))
        //                            {
        //                                HandleNetwork(node);
        //                            }
        //                            else
        //                            {
        //                                if ("load-balancer".Equals(nodeName))
        //                                {
        //                                    HandleLoadBalancer(node);
        //                                }
        //                                else
        //                                {
        //                                    if ("near-cache".Equals(nodeName))
        //                                    {
        //                                        HandleNearCache(node);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HandleNearCache(Node node)
        //{
        //    string name = GetAttribute(node, "name");
        //    NearCacheConfig nearCacheConfig = new NearCacheConfig();
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child);
        //        if ("max-size".Equals(nodeName))
        //        {
        //            nearCacheConfig.SetMaxSize(System.Convert.ToInt32(GetTextContent(child)));
        //        }
        //        else
        //        {
        //            if ("time-to-live-seconds".Equals(nodeName))
        //            {
        //                nearCacheConfig.SetTimeToLiveSeconds(System.Convert.ToInt32(GetTextContent(child)));
        //            }
        //            else
        //            {
        //                if ("max-idle-seconds".Equals(nodeName))
        //                {
        //                    nearCacheConfig.SetMaxIdleSeconds(System.Convert.ToInt32(GetTextContent(child)));
        //                }
        //                else
        //                {
        //                    if ("eviction-policy".Equals(nodeName))
        //                    {
        //                        nearCacheConfig.SetEvictionPolicy(GetTextContent(child));
        //                    }
        //                    else
        //                    {
        //                        if ("in-memory-format".Equals(nodeName))
        //                        {
        //                            nearCacheConfig.SetInMemoryFormat(InMemoryFormat.ValueOf(GetTextContent(child)));
        //                        }
        //                        else
        //                        {
        //                            if ("invalidate-on-change".Equals(nodeName))
        //                            {
        //                                nearCacheConfig.SetInvalidateOnChange(System.Boolean.Parse(GetTextContent(child)));
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    clientConfig.AddNearCacheConfig(name, nearCacheConfig);
        //}

        //private void HandleLoadBalancer(Node node)
        //{
        //    string type = GetAttribute(node, "type");
        //    if ("random".Equals(type))
        //    {
        //        clientConfig.SetLoadBalancer(new RandomLB());
        //    }
        //    else
        //    {
        //        if ("round-robin".Equals(type))
        //        {
        //            clientConfig.SetLoadBalancer(new RoundRobinLB());
        //        }
        //    }
        //}

        //private void HandleNetwork(Node node)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child);
        //        if ("cluster-members".Equals(nodeName))
        //        {
        //            HandleClusterMembers(child);
        //        }
        //        else
        //        {
        //            if ("smart-routing".Equals(nodeName))
        //            {
        //                clientConfig.SetSmartRouting(System.Boolean.Parse(GetTextContent(child)));
        //            }
        //            else
        //            {
        //                if ("redo-operation".Equals(nodeName))
        //                {
        //                    clientConfig.SetRedoOperation(System.Boolean.Parse(GetTextContent(child)));
        //                }
        //                else
        //                {
        //                    if ("connection-pool-size".Equals(nodeName))
        //                    {
        //                        clientConfig.SetConnectionPoolSize(System.Convert.ToInt32(GetTextContent(child)));
        //                    }
        //                    else
        //                    {
        //                        if ("connection-timeout".Equals(nodeName))
        //                        {
        //                            clientConfig.SetConnectionTimeout(System.Convert.ToInt32(GetTextContent(child)));
        //                        }
        //                        else
        //                        {
        //                            if ("connection-attempt-period".Equals(nodeName))
        //                            {
        //                                clientConfig.SetConnectionAttemptPeriod(System.Convert.ToInt32(GetTextContent(child)));
        //                            }
        //                            else
        //                            {
        //                                if ("connection-attempt-limit".Equals(nodeName))
        //                                {
        //                                    clientConfig.SetConnectionAttemptLimit(System.Convert.ToInt32(GetTextContent(child)));
        //                                }
        //                                else
        //                                {
        //                                    if ("socket-options".Equals(nodeName))
        //                                    {
        //                                        HandleSocketOptions(child);
        //                                    }
        //                                    else
        //                                    {
        //                                        if ("socket-interceptor".Equals(nodeName))
        //                                        {
        //                                            HandleSocketInterceptorConfig(node);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HandleSocketOptions(Node node)
        //{
        //    SocketOptions socketOptions = clientConfig.GetSocketOptions();
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child);
        //        if ("tcp-no-delay".Equals(nodeName))
        //        {
        //            socketOptions.SetTcpNoDelay(System.Boolean.Parse(GetTextContent(child)));
        //        }
        //        else
        //        {
        //            if ("keep-alive".Equals(nodeName))
        //            {
        //                socketOptions.SetKeepAlive(System.Boolean.Parse(GetTextContent(child)));
        //            }
        //            else
        //            {
        //                if ("reuse-address".Equals(nodeName))
        //                {
        //                    socketOptions.SetReuseAddress(System.Boolean.Parse(GetTextContent(child)));
        //                }
        //                else
        //                {
        //                    if ("linger-seconds".Equals(nodeName))
        //                    {
        //                        socketOptions.SetLingerSeconds(System.Convert.ToInt32(GetTextContent(child)));
        //                    }
        //                    else
        //                    {
        //                        if ("timeout".Equals(nodeName))
        //                        {
        //                            socketOptions.SetTimeout(System.Convert.ToInt32(GetTextContent(child)));
        //                        }
        //                        else
        //                        {
        //                            if ("buffer-size".Equals(nodeName))
        //                            {
        //                                socketOptions.SetBufferSize(System.Convert.ToInt32(GetTextContent(child)));
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HandleClusterMembers(Node node)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        if ("address".Equals(CleanNodeName(child)))
        //        {
        //            clientConfig.AddAddress(GetTextContent(child));
        //        }
        //    }
        //}

        ///// <exception cref="System.Exception"></exception>
        //private void HandleListeners(Node node)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        if ("listener".Equals(CleanNodeName(child)))
        //        {
        //            string className = GetTextContent(child);
        //            clientConfig.AddListenerConfig(new ListenerConfig(className));
        //        }
        //    }
        //}

        //private void HandleGroup(Node node)
        //{
        //    foreach (Node n in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string value = GetTextContent(n).Trim();
        //        string nodeName = CleanNodeName(n.GetNodeName());
        //        if ("name".Equals(nodeName))
        //        {
        //            clientConfig.GetGroupConfig().SetName(value);
        //        }
        //        else
        //        {
        //            if ("password".Equals(nodeName))
        //            {
        //                clientConfig.GetGroupConfig().SetPassword(value);
        //            }
        //        }
        //    }
        //}

        //private void HandleSerialization(Node node)
        //{
        //    SerializationConfig serializationConfig = ParseSerialization(node);
        //    clientConfig.SetSerializationConfig(serializationConfig);
        //}

        ///// <exception cref="System.Exception"></exception>
        //private void HandleProxyFactories(Node node)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child.GetNodeName());
        //        if ("proxy-factory".Equals(nodeName))
        //        {
        //            HandleProxyFactory(child);
        //        }
        //    }
        //}

        ///// <exception cref="System.Exception"></exception>
        //private void HandleProxyFactory(Node node)
        //{
        //    string service = GetAttribute(node, "service");
        //    string className = GetAttribute(node, "class-name");
        //    ProxyFactoryConfig proxyFactoryConfig = new ProxyFactoryConfig(className, service);
        //    clientConfig.AddProxyFactoryConfig(proxyFactoryConfig);
        //}

        //private void HandleSocketInterceptorConfig(Node node)
        //{
        //    SocketInterceptorConfig socketInterceptorConfig = ParseSocketInterceptorConfig(node);
        //    clientConfig.SetSocketInterceptorConfig(socketInterceptorConfig);
        //}

        ///// <exception cref="System.Exception"></exception>
        //private void HandleSecurity(Node node)
        //{
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child.GetNodeName());
        //        if ("login-credentials".Equals(nodeName))
        //        {
        //            HandleLoginCredentials(child);
        //        }
        //    }
        //}

        //private void HandleLoginCredentials(Node node)
        //{
        //    UsernamePasswordCredentials credentials = new UsernamePasswordCredentials();
        //    foreach (Node child in new AbstractXmlConfigHelper.IterableNodeList(node.GetChildNodes()))
        //    {
        //        string nodeName = CleanNodeName(child.GetNodeName());
        //        if ("username".Equals(nodeName))
        //        {
        //            credentials.SetUsername(GetTextContent(child));
        //        }
        //        else
        //        {
        //            if ("password".Equals(nodeName))
        //            {
        //                credentials.SetPassword(GetTextContent(child));
        //            }
        //        }
        //    }
        //    clientConfig.SetCredentials(credentials);
        //}
    }
}