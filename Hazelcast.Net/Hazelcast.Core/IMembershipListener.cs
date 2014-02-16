namespace Hazelcast.Core
{
    /// <summary>ICluster membership listener.</summary>
    /// <remarks>
    ///     ICluster membership listener.
    ///     The IMembershipListener will never be called concurrently and all MembershipListeners will receive the events
    ///     in the same order.
    /// </remarks>
    /// <seealso cref="IInitialMembershipListener">IInitialMembershipListener</seealso>
    /// <seealso cref="ICluster.AddMembershipListener(IMembershipListener)">ICluster.AddMembershipListener(IMembershipListener)</seealso>
    public interface IMembershipListener : IEventListener
    {
        /// <summary>Invoked when a new member is added to the cluster.</summary>
        /// <remarks>Invoked when a new member is added to the cluster.</remarks>
        /// <param name="membershipEvent">membership event</param>
        void MemberAdded(MembershipEvent membershipEvent);

        /// <summary>Invoked when an existing member leaves the cluster.</summary>
        /// <remarks>Invoked when an existing member leaves the cluster.</remarks>
        /// <param name="membershipEvent">membership event</param>
        void MemberRemoved(MembershipEvent membershipEvent);

        /// <summary>Invoked when an attribute of a member was changed.</summary>
        /// <param name="memberAttributeEvent">member attribute event</param>
        void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent);
    }
}