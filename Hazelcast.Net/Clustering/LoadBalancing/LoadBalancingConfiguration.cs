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

using System.Xml;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents the load balancing configuration.
    /// </summary>
    public class LoadBalancingConfiguration
    {
        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        public ServiceFactory<ILoadBalancer> LoadBalancer { get; } = new ServiceFactory<ILoadBalancer>(() => new RoundRobinLoadBalancer());

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static LoadBalancingConfiguration Parse(XmlNode node)
        {
            var configuration = new LoadBalancingConfiguration();

            var type = node.GetStringAttribute("type");

            if ("random".Equals(type))
            {
                configuration.LoadBalancer.Creator = () => new RandomLoadBalancer();
            }
            else if ("round-robin".Equals(type))
            {
                configuration.LoadBalancer.Creator = () => new RoundRobinLoadBalancer();
            }
            else
            {
                throw new ConfigurationException($"Unknown load balancer \"{type}\".");
            }

            return configuration;
        }
    }
}
