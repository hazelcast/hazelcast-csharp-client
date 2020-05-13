using System;
using Hazelcast.Clustering.LoadBalancing;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Represents the load balancing configuration.
    /// </summary>
    public class LoadBalancingConfiguration
    {
        private ILoadBalancer _loadBalancer;

        /// <summary>
        /// Gets or sets the load balancer instance.
        /// </summary>
        public ILoadBalancer LoadBalancer
        {
            get => _loadBalancer ?? (_loadBalancer = new RoundRobinLoadBalancer());
            set => _loadBalancer = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
