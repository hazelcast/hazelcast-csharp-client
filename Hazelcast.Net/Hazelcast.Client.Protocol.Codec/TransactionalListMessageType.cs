using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionalListMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalListMessageType TransactionallistAdd = new Hazelcast.Client.Protocol.Codec.TransactionalListMessageType(unchecked((int)(0x1301)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalListMessageType TransactionallistRemove = new Hazelcast.Client.Protocol.Codec.TransactionalListMessageType(unchecked((int)(0x1302)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalListMessageType TransactionallistSize = new Hazelcast.Client.Protocol.Codec.TransactionalListMessageType(unchecked((int)(0x1303)));

		private readonly int id;

		internal TransactionalListMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionalListMessageType.id;
		}
	}
}
