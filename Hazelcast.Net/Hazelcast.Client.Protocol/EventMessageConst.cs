namespace Hazelcast.Client.Protocol
{
	/// <summary>Message type ids of event responses in client protocol.</summary>
	/// <remarks>
	/// Message type ids of event responses in client protocol. They also used to bind a request to event inside Request
	/// annotation.
	/// <p/>
	/// Event response classes are defined
	/// <see cref="com.hazelcast.client.impl.protocol.template.EventResponseTemplate"/>
	/// <p/>
	/// see
	/// <see cref="com.hazelcast.client.impl.protocol.template.ClientMessageTemplate#membershipListener()"/>
	/// for  a sample usage of events in a request.
	/// </remarks>
	public sealed class EventMessageConst
	{
		public const int EventMember = 200;

		public const int EventMemberlist = 201;

		public const int EventMemberattributechange = 202;

		public const int EventEntry = 203;

		public const int EventItem = 204;

		public const int EventTopic = 205;

		public const int EventPartitionlost = 206;

		public const int EventDistributedobject = 207;

		public const int EventCacheinvalidation = 208;

		public const int EventMappartitionlost = 209;

		public const int EventCache = 210;
	}
}
