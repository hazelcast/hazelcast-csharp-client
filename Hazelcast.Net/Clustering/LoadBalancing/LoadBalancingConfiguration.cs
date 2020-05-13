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
