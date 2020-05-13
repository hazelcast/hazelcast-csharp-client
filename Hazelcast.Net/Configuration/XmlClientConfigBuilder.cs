
// FIXME remove this file
//// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
////
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
////
//// http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using System;
//using System.IO;
//using System.Xml;
//using Hazelcast.Clustering;
//using Hazelcast.Clustering.LoadBalancing;
//using Hazelcast.Core;
//using Hazelcast.Exceptions;
//using Hazelcast.NearCaching;
//using Hazelcast.Networking;
//using Hazelcast.Security;
//using Hazelcast.Serialization;

//namespace Hazelcast.Configuration
//{
//    /// <summary>
//    /// Loads the <see cref="HazelcastConfiguration"/> using XML.
//    /// </summary>
//    public class XmlClientConfigBuilder
//    {
//        // TODO: this class should be massively refactored
//        // + move to .NET Core configuration style
//        // + move to JSON

//        private readonly XmlDocument _document = new XmlDocument();

//        private HazelcastConfiguration _hazelcastConfiguration;

//        internal XmlClientConfigBuilder(TextReader reader)
//        {
//            try
//            {
//                _document.Load(reader);
//                reader.Dispose();
//            }
//            catch (Exception)
//            {
//                throw new InvalidOperationException("Could not parse configuration file, giving up.");
//            }
//        }

//        private XmlClientConfigBuilder(string configFile = null)
//        {
//            if (configFile == null)
//            {
//                configFile = Environment.GetEnvironmentVariable("hazelcast.client.config");
//            }
//            try
//            {
//                TextReader input = null;
//                if (configFile != null)
//                {
//                    if (File.Exists(configFile))
//                    {
//                        input = File.OpenText(configFile);
//                    }
//                    else
//                    {
//                        throw new ConfigurationException($"Could not open file {configFile}.");
//                    }
//                }
//                if (input == null)
//                {
//                    configFile = Directory.GetCurrentDirectory() + "\\hazelcast-client.xml";
//                    if (File.Exists(configFile))
//                    {
//                        input = File.OpenText(configFile);
//                    }
//                    else
//                    {
//                        input = new StringReader(Resources.hazelcast_client_default);
//                    }
//                }

//                try
//                {
//                    _document.Load(input);
//                }
//                finally
//                {
//                    input.Dispose();
//                }
//            }
//            catch (Exception e)
//            {
//                throw new ConfigurationException("Exception while reading configuration.", e);
//            }
//        }

//        /// <summary>
//        /// Build a <see cref="HazelcastConfiguration"/> from an XML file
//        /// </summary>
//        /// <param name="configFile">hazelcast client XML config file</param>
//        /// <returns>ClientConfig</returns>
//        public static HazelcastConfiguration Build(string configFile = null)
//        {
//            return new XmlClientConfigBuilder(configFile).Init();
//        }

//        /// <summary>
//        /// Build a <see cref="HazelcastConfiguration"/> from an XML file
//        /// </summary>
//        /// <param name="reader">Text reader to provide hazelcast client XML</param>
//        /// <returns>ClientConfig</returns>
//        public static HazelcastConfiguration Build(TextReader reader)
//        {
//            return new XmlClientConfigBuilder(reader).Init();
//        }

//        /// <summary>
//        /// Creates a <see cref="HazelcastConfiguration"/> using the XML content
//        /// </summary>
//        /// <returns></returns>
//        protected HazelcastConfiguration Init()
//        {
//            _hazelcastConfiguration = new HazelcastConfiguration();
//            lock (_hazelcastConfiguration)
//            {
//                HandleConfig(_document.DocumentElement); //PARSE
//            }
//            return _hazelcastConfiguration;
//        }

//        /// <exception cref="Exception"></exception>
//        private void HandleConfig(XmlElement docElement)
//        {
//            foreach (XmlNode node in docElement.ChildNodes)
//            {
//                var nodeName = node.GetCleanName();
//                switch (nodeName)
//                {
//                    case "security":
//                        _hazelcastConfiguration.Security = SecurityConfiguration.Parse(node);
//                        break;
//                    case "proxy-factories":
//                        // skip it - not supported for now
//                        // would contain <proxy-factory service="" class-name="" />
//                        // used by DistributedObjectFactory to create stuff
//                        //HandleProxyFactories(node);
//                        break;
//                    case "serialization":
//                        _hazelcastConfiguration.Serialization = SerializationConfiguration.Parse(node);
//                        break;
//                    case "cluster":
//                        var clusterName = node.GetStringAttribute("name");
//                        if (!string.IsNullOrWhiteSpace(clusterName)) _hazelcastConfiguration.ClusterName = clusterName;
//                        break;
//                    case "listeners":
//                        _hazelcastConfiguration.Cluster = ClusterConfiguration.Parse(node);
//                        break;
//                    case "network":
//                        _hazelcastConfiguration.Networking = NetworkingConfiguration.Parse(node);
//                        break;
//                    case "load-balancer":
//                        _hazelcastConfiguration.LoadBalancing = LoadBalancingConfiguration.Parse(node);
//                        break;
//                    case "near-cache":
//                        _hazelcastConfiguration.NearCache = NearCacheConfigurations.Parse(node);
//                        break;
//                    case "connection-strategy":
//                        throw new NotImplementedException();
//                }
//            }
//        }
//    }
//}