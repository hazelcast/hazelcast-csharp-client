// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Security;

namespace Hazelcast.Client
{
    /// <summary>
    ///     Hazelcast Client enables you to do all Hazelcast operations without
    ///     being a member of the cluster.
    /// </summary>
    /// <remarks>
    ///     Hazelcast Client enables you to do all Hazelcast operations without
    ///     being a member of the cluster. It connects to one of the
    ///     cluster members and delegates all cluster wide operations to it.
    ///     When the connected cluster member dies, client will
    ///     automatically switch to another live member.
    /// </remarks>
    public sealed partial class HazelcastClient
    {
        private static readonly ConcurrentDictionary<string, HazelcastClient> Clients =
            new ConcurrentDictionary<string, HazelcastClient>();

        private static readonly AtomicInteger ClientId = new AtomicInteger();


        /// <summary>
        ///     Gets all Hazelcast clients.
        /// </summary>
        /// <returns>ICollection&lt;IHazelcastInstance&gt;</returns>
        public static ICollection<IHazelcastInstance> GetAllHazelcastClients()
        {
            return (ICollection<IHazelcastInstance>) Clients.Values;
        }

        /// <summary>
        ///     Creates a new hazelcast client using default configuration.
        /// </summary>
        /// <remarks>
        ///     Creates a new hazelcast client using default configuration.
        /// </remarks>
        /// <returns>IHazelcastInstance.</returns>
        /// <example>
        ///     <code>
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient();
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        /// </example>
        public static IHazelcastInstance NewHazelcastClient()
        {
            return NewHazelcastClient(XmlClientConfigBuilder.Build());
        }

        /// <summary>
        ///     Creates a new hazelcast client using the given configuration xml file
        /// </summary>
        /// <param name="configFile">The configuration file with full or relative path.</param>
        /// <returns>IHazelcastInstance.</returns>
        /// <example>
        ///     <code>
        ///     //Full path
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(@"C:\Users\user\Hazelcast.Net\hazelcast-client.xml");
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        ///     
        ///     //relative path
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(@"..\Hazelcast.Net\Resources\hazelcast-client.xml");
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        /// </example>
        public static IHazelcastInstance NewHazelcastClient(string configFile)
        {
            return NewHazelcastClient(XmlClientConfigBuilder.Build(configFile));
        }

        /// <summary>
        ///     Creates a new hazelcast client using the given configuration object created programmaticly.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>IHazelcastInstance.</returns>
        /// <code>
        ///     var clientConfig = new ClientConfig();
        ///     //configure clientConfig ...
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(clientConfig);
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        public static IHazelcastInstance NewHazelcastClient(ClientConfig config)
        {
            if (config == null)
            {
                config = XmlClientConfigBuilder.Build();
            }
            var client = new HazelcastClient(config);
            client.Start();
            Clients.TryAdd(client._instanceName, client);
            return client;
        }

        /// <summary>
        /// Shutdown the provided client and remove it from the managed list
        /// </summary>
        /// <param name="instanceName">the hazelcast client instance name</param>
        public static void Shutdown(string instanceName)
        {
            Clients.TryRemove(instanceName, out var client);
            if (client == null)
            {
                return;
            }
            try
            {
                ((IHazelcastInstance) client).Shutdown();
            }
            catch
            {
                //ignored
            }
        }

        /// <summary>
        /// Shutdowns all Hazelcast Clients .
        /// </summary>
        public static void ShutdownAll()
        {
            foreach (var client in Clients.Values)
            {
                try
                {
                    ((IHazelcastInstance) client).Shutdown();
                }
                catch
                {
                    // ignored
                }
            }
            Clients.Clear();
        }
    }
}