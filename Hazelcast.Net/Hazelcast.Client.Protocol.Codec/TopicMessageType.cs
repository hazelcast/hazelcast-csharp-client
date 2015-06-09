using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TopicMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TopicMessageType TopicPublish = new Hazelcast.Client.Protocol.Codec.TopicMessageType(unchecked((int)(0x0401)));

		public static readonly Hazelcast.Client.Protocol.Codec.TopicMessageType TopicAddmessagelistener = new Hazelcast.Client.Protocol.Codec.TopicMessageType(unchecked((int)(0x0402)));

		public static readonly Hazelcast.Client.Protocol.Codec.TopicMessageType TopicRemovemessagelistener = new Hazelcast.Client.Protocol.Codec.TopicMessageType(unchecked((int)(0x0403)));

		private readonly int id;

		internal TopicMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TopicMessageType.id;
		}
	}
}
