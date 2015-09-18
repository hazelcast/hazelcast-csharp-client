using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Connection
{
    internal class Router
    {
        private readonly ILoadBalancer _loadBalancer;

        internal Router(ILoadBalancer loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public virtual Address Next()
        {
            IMember member = _loadBalancer.Next();
            return member == null ? null : member.GetAddress();
        }
    }
}