using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class ReplicatedMapMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapPut = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e01)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapSize = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e02)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapIsempty = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e03)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapContainskey = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e04)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapContainsvalue = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e05)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapGet = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e06)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapRemove = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e07)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapPutall = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e08)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapClear = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e09)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapAddentrylistenertokeywithpredicate = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0a)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapAddentrylistenerwithpredicate = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0b)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapAddentrylistenertokey = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0c)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapAddentrylistener = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0d)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapRemoveentrylistener = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0e)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapKeyset = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e0f)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapValues = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e10)));

		public static readonly Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType ReplicatedmapEntryset = new Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType(unchecked((int)(0x0e11)));

		private readonly int id;

		internal ReplicatedMapMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.ReplicatedMapMessageType.id;
		}
	}
}
