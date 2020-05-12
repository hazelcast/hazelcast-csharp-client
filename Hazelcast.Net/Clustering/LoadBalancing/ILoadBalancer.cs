using System;
using System.Collections.Generic;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Defines a service that can load-balance requests to different members of a cluster.
    /// </summary>
    public interface ILoadBalancer
    {
        /// <summary>
        /// Gets the number of members.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Selects a member.
        /// </summary>
        /// <returns>The unique identifier of the selected member.</returns>
        Guid Select();

        /// <summary>
        /// Notifies the load balancer of a new set of members.
        /// </summary>
        /// <param name="memberIds">The identifiers of the members.</param>
        /// <remarks>
        /// <para>The new set of members fully replace existing members.</para>
        /// </remarks>
        void NotifyMembers(IEnumerable<Guid> memberIds);
    }
}
