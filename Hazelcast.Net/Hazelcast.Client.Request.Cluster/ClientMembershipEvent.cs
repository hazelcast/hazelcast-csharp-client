using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    [Serializable]
    public sealed class ClientMembershipEvent : IIdentifiedDataSerializable
    {
        public const int MemberAdded = MembershipEvent.MemberAdded;

        public const int MemberRemoved = MembershipEvent.MemberRemoved;

        private int eventType;
        private IMember member;

        public ClientMembershipEvent()
        {
        }

        public ClientMembershipEvent(IMember member, int eventType)
        {
            this.member = member;
            this.eventType = eventType;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            member.WriteData(output);
            output.WriteInt(eventType);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            member = new Member();
            member.ReadData(input);
            eventType = input.ReadInt();
        }

        public int GetFactoryId()
        {
            return ClusterDataSerializerHook.FId;
        }

        public int GetId()
        {
            return ClusterDataSerializerHook.MembershipEvent;
        }

        /// <summary>Returns the membership event type; #MEMBER_ADDED or #MEMBER_REMOVED</summary>
        /// <returns>the membership event type</returns>
        public int GetEventType()
        {
            return eventType;
        }

        /// <summary>Returns the removed or added member.</summary>
        /// <remarks>Returns the removed or added member.</remarks>
        /// <returns>member which is removed/added</returns>
        public IMember GetMember()
        {
            return member;
        }
    }
}