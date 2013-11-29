using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Connection
{
    internal class Router
    {
        private readonly LoadBalancer _loadBalancer;

        internal Router(LoadBalancer loadBalancer)
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