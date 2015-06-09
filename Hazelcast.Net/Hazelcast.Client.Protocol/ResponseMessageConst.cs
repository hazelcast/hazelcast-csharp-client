namespace Hazelcast.Client.Protocol
{
	/// <summary>Message type ids of responses in client protocol.</summary>
	/// <remarks>
	/// Message type ids of responses in client protocol. They also used to bind a request to a response inside Request
	/// annotation.
	/// <p/>
	/// Response classes are defined
	/// <see cref="com.hazelcast.client.impl.protocol.template.ResponseTemplate"/>
	/// <p/>
	/// see
	/// <see cref="com.hazelcast.client.impl.protocol.template.ClientMessageTemplate#membershipListener()"/>
	/// for  a sample usage of responses in a request.
	/// </remarks>
	public sealed class ResponseMessageConst
	{
		public const int Void = 100;

		public const int Boolean = 101;

		public const int Integer = 102;

		public const int Long = 103;

		public const int String = 104;

		public const int Data = 105;

		public const int ListData = 106;

		public const int MapIntData = 107;

		public const int MapDataData = 108;

		public const int Authentication = 109;

		public const int Partitions = 110;

		public const int Exception = 111;

		public const int DistributedObject = 112;

		public const int EntryView = 113;

		public const int JobProcessInfo = 114;

		public const int SetData = 115;

		public const int SetEntry = 116;

		public const int MapIntBoolean = 117;
	}
}
