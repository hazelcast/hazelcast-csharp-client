using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Occurs when a member is added to or removed from the cluster.
    /// </summary>
    public class MembershipEvent
    {
        public MembershipEvent(MembershipEventType eventType, MemberInfo member)
        {
            EventType = eventType;
            Member = member;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public MembershipEventType EventType { get; }

        /// <summary>
        /// Gets the member.
        /// </summary>
        public MemberInfo Member { get; }
    }
}
