using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     A event that is send when a
    ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
    ///     registers itself on a
    ///     <see cref="ICluster">ICluster</see>
    ///     . For more
    ///     information see the
    ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
    ///     .
    /// </summary>
    /// <seealso cref="IInitialMembershipListener">IInitialMembershipListener</seealso>
    /// <seealso cref="IMembershipListener">IMembershipListener</seealso>
    /// <seealso cref="MembershipEvent">MembershipEvent</seealso>
    [Serializable]
    public class InitialMembershipEvent : EventObject
    {
        private readonly ICollection<IMember> members;

        public InitialMembershipEvent(ICluster cluster, ICollection<IMember> members) : base(cluster)
        {
            this.members = members;
        }

        /// <summary>
        ///     Returns an immutable set of ordered members at the moment this
        ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
        ///     is
        ///     registered. See
        ///     <see cref="ICluster.GetMembers()">ICluster.GetMembers()</see>
        ///     for more information.
        /// </summary>
        /// <returns>a set of members.</returns>
        public virtual ICollection<IMember> GetMembers()
        {
            return members;
        }

        /// <summary>Returns the cluster of the event.</summary>
        /// <remarks>Returns the cluster of the event.</remarks>
        /// <returns>the cluster of the event.</returns>
        public virtual ICluster GetCluster()
        {
            return (ICluster) GetSource();
        }

        public override string ToString()
        {
            return "MembershipInitializeEvent {" + members + "}";
        }
    }
}