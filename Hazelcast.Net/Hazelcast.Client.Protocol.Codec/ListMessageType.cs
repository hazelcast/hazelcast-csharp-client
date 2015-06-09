using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class ListMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListSize = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0501)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListContains = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0502)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListContainsall = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0503)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListAdd = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0504)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListRemove = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0505)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListAddall = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0506)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListCompareandremoveall = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0507)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListCompareandretainall = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0508)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListClear = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0509)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListGetall = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050a)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListAddlistener = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050b)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListRemovelistener = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050c)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListIsempty = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050d)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListAddallwithindex = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050e)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListGet = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x050f)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListSet = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0510)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListAddwithindex = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0511)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListRemovewithindex = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0512)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListLastindexof = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0513)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListIndexof = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0514)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListSub = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0515)));

		public static readonly Hazelcast.Client.Protocol.Codec.ListMessageType ListIterator = new Hazelcast.Client.Protocol.Codec.ListMessageType(unchecked((int)(0x0516)));

		private readonly int id;

		internal ListMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.ListMessageType.id;
		}
	}
}
