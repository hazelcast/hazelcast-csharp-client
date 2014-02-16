using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    public sealed class ClientMembershipEvent : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        public const int MemberAdded = MembershipEvent.MemberAdded;

        public const int MemberRemoved = MembershipEvent.MemberRemoved;
        
        public const int MemberAttributeChanged = MembershipEvent.MemberAttributeChanged;


        private int eventType;
        private IMember member;
        private MemberAttributeChange memberAttributeChange;

        public ClientMembershipEvent(){}

        public ClientMembershipEvent(IMember member, int eventType):this(member,null,eventType) {}

        public ClientMembershipEvent(IMember member, MemberAttributeChange memberAttributeChange,int eventType)
        {
            this.eventType = eventType;
            this.member = member;
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


        /// <summary>
        /// Returns the member attribute chance operation to execute 
        /// if event type is #MEMBER_ATTRIBUTE_CHANGED.
        /// </summary>
        /// <returns>MemberAttributeChange to execute</returns>
        public MemberAttributeChange GetMemberAttributeChange()
        {
            return memberAttributeChange;
        }


    }
}