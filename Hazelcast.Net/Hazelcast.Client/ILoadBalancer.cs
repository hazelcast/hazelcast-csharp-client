using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Client
{
    /// <summary>
    ///     <see cref="ILoadBalancer">ILoadBalancer</see>
    ///     allows you to send operations to one of a number of endpoints(Members).
    ///     It is up to the implementation to use different load balancing policies. If IClient is
    ///     <see cref="ClientConfig#smart">ClientConfig#smart</see>
    ///     ,
    ///     only the operations that are not key based will be router to the endpoint returned by the Load Balancer.
    ///     If it is not
    ///     <see cref="ClientConfig#smart">ClientConfig#smart</see>
    ///     ,
    ///     <see cref="ILoadBalancer">ILoadBalancer</see>
    ///     will not be used.
    /// </summary>
    public interface ILoadBalancer
    {
        void Init(ICluster cluster, ClientConfig config);

        /// <summary>Returns the next member to route to</summary>
        /// <returns>Returns the next member or null if no member is available</returns>
        IMember Next();
    }
}