using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class TransactionalMapMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapContainskey = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1001)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapGet = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1002)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapGetforupdate = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1003)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapSize = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1004)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapIsempty = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1005)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapPut = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1006)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapSet = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1007)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapPutifabsent = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1008)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapReplace = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1009)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapReplaceifsame = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100a)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapRemove = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100b)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapDelete = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100c)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapRemoveifsame = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100d)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapKeyset = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100e)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapKeysetwithpredicate = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x100f)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapValues = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1010)));

		public static readonly Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType TransactionalmapValueswithpredicate = new Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType(unchecked((int)(0x1011)));

		private readonly int id;

		internal TransactionalMapMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.TransactionalMapMessageType.id;
		}
	}
}
