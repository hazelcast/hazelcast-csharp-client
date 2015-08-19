using Hazelcast.Client.Spi;

namespace Hazelcast.Core
{
    public class MemberAttributeEvent : MembershipEvent
    {
        private readonly string key;
        private readonly Member member;
        private readonly MemberAttributeOperationType operationType;
        private readonly object value;

        public MemberAttributeEvent() : base(null, null, MemberAttributeChanged, null)
        {
        }

        public MemberAttributeEvent(ICluster cluster, IMember member, MemberAttributeOperationType operationType,
            string key, object value)
            : base(cluster, member, MemberAttributeChanged, null)
        {
            this.member = (Member) member;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        public string GetKey()
        {
            return key;
        }

        public override IMember GetMember()
        {
            return member;
        }

        public MemberAttributeOperationType GetOperationType()
        {
            return operationType;
        }

        public object GetValue()
        {
            return value;
        }
    }
}