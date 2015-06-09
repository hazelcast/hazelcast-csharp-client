using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class QueueMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueOffer = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0301)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueuePut = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0302)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueSize = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0303)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueRemove = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0304)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueuePoll = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0305)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueTake = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0306)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueuePeek = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0307)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueIterator = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0308)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueDrainto = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0309)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueDraintomaxsize = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030a)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueContains = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030b)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueContainsall = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030c)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueCompareandremoveall = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030d)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueCompareandretainall = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030e)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueClear = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x030f)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueAddall = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0310)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueAddlistener = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0311)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueRemovelistener = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0312)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueRemainingcapacity = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0313)));

		public static readonly Hazelcast.Client.Protocol.Codec.QueueMessageType QueueIsempty = new Hazelcast.Client.Protocol.Codec.QueueMessageType(unchecked((int)(0x0314)));

		private readonly int id;

		internal QueueMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.QueueMessageType.id;
		}
	}
}
