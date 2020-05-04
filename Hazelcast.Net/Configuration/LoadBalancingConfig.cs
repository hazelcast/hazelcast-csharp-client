using Hazelcast.Clustering.LoadBalancing;

namespace Hazelcast.Configuration
{
    public class LoadBalancingConfig
    {
        /// <summary>Used to distribute the operations to multiple Endpoints.</summary>
        private ILoadBalancer _loadBalancer = new RoundRobinLoadBalancer();

        internal virtual ILoadBalancer GetLoadBalancer()
        {
            return _loadBalancer;
        }

        public virtual LoadBalancingConfig SetLoadBalancer(ILoadBalancer loadBalancer)
        {
            _loadBalancer = loadBalancer;
            return this;
        }

        public virtual LoadBalancingConfig SetRandomLoadBalancer()
            => SetLoadBalancer(new RandomLoadBalancer());

        public virtual LoadBalancingConfig SetRoundRobinLoadBalancer()
            => SetLoadBalancer(new RoundRobinLoadBalancer());

        public static ILoadBalancer DefaultLoadBalancer
            => new RoundRobinLoadBalancer();
    }
}
