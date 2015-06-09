using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionalSetMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType TransactionalsetAdd = new Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType(unchecked((int)(0x1201)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType TransactionalsetRemove = new Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType(unchecked((int)(0x1202)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType TransactionalsetSize = new Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType(unchecked((int)(0x1203)));

		private readonly int id;

		internal TransactionalSetMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionalSetMessageType.id;
		}
	}
}
