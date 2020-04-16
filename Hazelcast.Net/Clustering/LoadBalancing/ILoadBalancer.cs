using System;
using System.Collections;
using System.Collections.Generic;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Defines a service that can load-balance requests to different members of a cluster.
    /// </summary>
    public interface ILoadBalancer
    {
        /// <summary>
        /// Selects a member.
        /// </summary>
        /// <returns>The unique identifier of the selected member.</returns>
        Guid Select();

        // FIXME document / cleanup
        void NotifyMembers(IEnumerable<Guid> memberIds);

        /// <summary>
        /// Notifies the load balancer that a member was added to the cluster.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member.</param>
        void NotifyAddedMember(Guid memberId);

        /// <summary>
        /// Notifies the load balancer that a member was removed from the cluster.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member.</param>
        void NotifyRemovedMember(Guid memberId);
    }
}
