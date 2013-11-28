namespace Hazelcast.Core
{
    /// <summary>
    ///     The InitializingMembershipListener is a
    ///     <see cref="IMembershipListener">IMembershipListener</see>
    ///     that will first receives a
    ///     <see cref="InitialMembershipEvent">InitialMembershipEvent</see>
    ///     when it is registered so it immediately knows which members are available. After
    ///     that event has been received, it will receive the normal MembershipEvents.
    ///     When the InitializingMembershipListener already is registered on a
    ///     <see cref="ICluster">ICluster</see>
    ///     and is registered again on the same
    ///     ICluster instance, it will not receive an additional MembershipInitializeEvent. So this is a once only event.
    /// </summary>
    /// <seealso cref="ICluster.AddMembershipListener(IMembershipListener)">ICluster.AddMembershipListener(IMembershipListener)</seealso>
    /// <seealso cref="MembershipEvent.GetMembers()">MembershipEvent.GetMembers()</seealso>
    public interface IInitialMembershipListener : IMembershipListener
    {
        /// <summary>Is called when this listener is registered.</summary>
        /// <remarks>Is called when this listener is registered.</remarks>
        /// <param name="event">the MembershipInitializeEvent</param>
        void Init(InitialMembershipEvent membershipEvent);
    }
}