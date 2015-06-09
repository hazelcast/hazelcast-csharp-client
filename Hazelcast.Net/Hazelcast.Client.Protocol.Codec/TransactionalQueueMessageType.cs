using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionalQueueMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType TransactionalqueueOffer = new Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType(unchecked((int)(0x1401)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType TransactionalqueueTake = new Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType(unchecked((int)(0x1402)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType TransactionalqueuePoll = new Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType(unchecked((int)(0x1403)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType TransactionalqueuePeek = new Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType(unchecked((int)(0x1404)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType TransactionalqueueSize = new Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType(unchecked((int)(0x1405)));

		private readonly int id;

		internal TransactionalQueueMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionalQueueMessageType.id;
		}
	}
}
