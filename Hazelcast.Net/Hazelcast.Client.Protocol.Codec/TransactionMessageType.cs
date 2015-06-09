using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionMessageType TransactionCommit = new Hazelcast.Client.Protocol.Codec.TransactionMessageType(unchecked((int)(0x1701)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionMessageType TransactionCreate = new Hazelcast.Client.Protocol.Codec.TransactionMessageType(unchecked((int)(0x1702)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionMessageType TransactionRollback = new Hazelcast.Client.Protocol.Codec.TransactionMessageType(unchecked((int)(0x1703)));

		private readonly int id;

		internal TransactionMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionMessageType.id;
		}
	}
}
