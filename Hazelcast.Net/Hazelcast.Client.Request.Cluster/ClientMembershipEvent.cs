using Hazelcast.Client.Request.Cluster;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Cluster
{
	public sealed class ClientMembershipEvent : IIdentifiedDataSerializable
	{
		public const int MEMBER_ADDED = MembershipEvent.MEMBER_ADDED;

		public const int MEMBER_REMOVED = MembershipEvent.MEMBER_REMOVED;

		public const int MEMBER_ATTRIBUTE_CHANGED = MembershipEvent.MEMBER_ATTRIBUTE_CHANGED;

		private IMember member;

		private MemberAttributeChange memberAttributeChange;

		private int eventType;

		public ClientMembershipEvent()
		{
		}

		public ClientMembershipEvent(IMember member, int eventType) : this(member, null, 
			eventType)
		{
		}

		public ClientMembershipEvent(IMember member, MemberAttributeChange memberAttributeChange
			) : this(member, memberAttributeChange, MEMBER_ATTRIBUTE_CHANGED)
		{
		}

		private ClientMembershipEvent(IMember member, MemberAttributeChange memberAttributeChange
			, int eventType)
		{
			this.member = member;
			this.eventType = eventType;
			this.memberAttributeChange = memberAttributeChange;
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
		/// if event type is
		/// <see cref="MEMBER_ATTRIBUTE_CHANGED">MEMBER_ATTRIBUTE_CHANGED</see>
		/// .
		/// </summary>
		/// <returns>MemberAttributeChange to execute</returns>
		public MemberAttributeChange GetMemberAttributeChange()
		{
			return memberAttributeChange;
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
			member = new MemberImpl();
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
			return ClusterDataSerializerHook.F_ID;
		}

		public int GetId()
		{
			return ClusterDataSerializerHook.MEMBERSHIP_EVENT;
		}
	}
}
