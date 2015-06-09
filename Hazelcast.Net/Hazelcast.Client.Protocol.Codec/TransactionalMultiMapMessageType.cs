using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionalMultiMapMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapPut = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1101)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapGet = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1102)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapRemove = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1103)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapRemoveentry = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1104)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapValuecount = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1105)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType TransactionalmultimapSize = new Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType(unchecked((int)(0x1106)));

		private readonly int id;

		internal TransactionalMultiMapMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionalMultiMapMessageType.id;
		}
	}
}
