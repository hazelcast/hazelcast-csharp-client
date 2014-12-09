using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    internal sealed class ClientMembershipEvent : IdentifiedDataSerializable, IIdentifiedDataSerializable
    {
        public const int MemberAdded = MembershipEvent.MemberAdded;
        public const int MemberRemoved = MembershipEvent.MemberRemoved;
        public const int MemberAttributeChanged = MembershipEvent.MemberAttributeChanged;
        private int eventType;
        private IMember member;
        private MemberAttributeChange memberAttributeChange;

        public ClientMembershipEvent()
        {
        }

        public ClientMembershipEvent(IMember member, int eventType) : this(member, null, eventType)
        {
        }

        public ClientMembershipEvent(IMember member, MemberAttributeChange memberAttributeChange)
            : this(member, memberAttributeChange, MemberAttributeChanged)
        {
        }

        private ClientMembershipEvent(IMember member, MemberAttributeChange memberAttributeChange, int eventType)
        {
            this.member = member;
            this.eventType = eventType;
            this.memberAttributeChange = memberAttributeChange;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            member.WriteData(output);
            output.WriteInt(eventType);
            output.WriteBoolean(memberAttributeChange != null);
            if (memberAttributeChange != null)
            {
                memberAttributeChange.WriteData(output);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            member = new Member();
            member.ReadData(input);
            eventType = input.ReadInt();
            if (input.ReadBoolean())
            {
                memberAttributeChange = new MemberAttributeChange();
                memberAttributeChange.ReadData(input);
            }
        }

        public int GetFactoryId()
        {
            return ClusterDataSerializerHook.FId;
        }

        public int GetId()
        {
            return ClusterDataSerializerHook.MembershipEvent;
        }

        public int GetEventType()
        {
            return eventType;
        }

        public IMember GetMember()
        {
            return member;
        }

        public MemberAttributeChange GetMemberAttributeChange()
        {
            return memberAttributeChange;
        }
    }
}