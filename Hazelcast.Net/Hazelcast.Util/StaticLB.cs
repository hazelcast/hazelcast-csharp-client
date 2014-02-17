using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    internal class StaticLB : LoadBalancer
    {
        private readonly IMember member;

        public StaticLB(IMember member)
        {
            this.member = member;
        }

        public virtual void Init(ICluster cluster, ClientConfig config)
        {
        }

        public virtual IMember Next()
        {
            return member;
        }
    }
}