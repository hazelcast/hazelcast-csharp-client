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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Security;
using Hazelcast.Serialization;

namespace Hazelcast
{
    /// <summary>
    /// Main configuration to setup a Hazelcast Client.
    /// </summary>
    public sealed class HazelcastConfiguration
    {
        /// <summary>
        /// Gets the default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets or sets the unique name of the cluster.
        /// </summary>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets the instance name. TODO: what is it?
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the FIXME ??? + why is it concurrent ???
        /// </summary>
        public IDictionary<string, string> Properties { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets or sets the FIXME ???
        /// </summary>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Whether to start the client asynchronously. TODO: still used?
        /// </summary>
        public bool AsyncStart { get; set; }

        /// <summary>
        /// Gets or sets the cluster configuration.
        /// </summary>
        public ClusterConfiguration Cluster { get; set; } = new ClusterConfiguration();

        /// <summary>
        /// Gets the logging configuration.
        /// </summary>
        public LoggingConfiguration Logging { get; } = new LoggingConfiguration();

        /// <summary>
        /// Gets or sets the networking configuration.
        /// </summary>
        public NetworkingConfiguration Networking { get; set; } = new NetworkingConfiguration();

        /// <summary>
        /// Gets or sets the security configuration.
        /// </summary>
        public SecurityConfiguration Security { get; set;  } = new SecurityConfiguration();

        /// <summary>
        /// Gets or sets the load balancing configuration.
        /// </summary>
        public LoadBalancingConfiguration LoadBalancing { get; set; } = new LoadBalancingConfiguration();

        /// <summary>
        /// Gets the serialization configuration.
        /// </summary>
        public SerializationConfiguration Serialization { get; set; } = new SerializationConfiguration();

        /// <summary>
        /// Gets or sets the NearCache configuration.
        /// </summary>
        public NearCacheConfigurations NearCache { get; set;  } = new NearCacheConfigurations();


        // TODO: document
        public static HazelcastConfiguration CreateDefault() => Parse();

        public static HazelcastConfiguration Parse(string configFile = null)
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
                    var document = new XmlDocument();
                    document.Load(input);
                    return Parse(document);
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

        public static HazelcastConfiguration Parse(TextReader reader)
        {
            try
            {
                var document = new XmlDocument();
                document.Load(reader);
                return Parse(document);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Exception while reading configuration.", e);
            }
        }

        public static HazelcastConfiguration Parse(XmlDocument document)
        {
            var configuration = new HazelcastConfiguration();

            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var nodeName = node.GetCleanName();
                switch (nodeName)
                {
                    case "security":
                        configuration.Security = SecurityConfiguration.Parse(node);
                        break;
                    case "proxy-factories":
                        // skip it - not supported for now
                        // would contain <proxy-factory service="" class-name="" />
                        // used by DistributedObjectFactory to create stuff
                        //HandleProxyFactories(node);
                        break;
                    case "serialization":
                        configuration.Serialization = SerializationConfiguration.Parse(node);
                        break;
                    case "cluster":
                        var clusterName = node.GetStringAttribute("name");
                        if (!string.IsNullOrWhiteSpace(clusterName)) configuration.ClusterName = clusterName;
                        break;
                    case "listeners":
                        configuration.Cluster = ClusterConfiguration.Parse(node);
                        break;
                    case "network":
                        configuration.Networking = NetworkingConfiguration.Parse(node);
                        break;
                    case "load-balancer":
                        configuration.LoadBalancing = LoadBalancingConfiguration.Parse(node);
                        break;
                    case "near-cache":
                        configuration.NearCache = NearCacheConfigurations.Parse(node);
                        break;
                    case "connection-strategy":
                        throw new NotImplementedException();
                }
            }

            return configuration;
        }
    }
}