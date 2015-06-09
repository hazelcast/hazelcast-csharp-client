using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class AtomicLongMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongApply = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a01)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongAlter = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a02)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongAlterandget = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a03)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongGetandalter = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a04)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongAddandget = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a05)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongCompareandset = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a06)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongDecrementandget = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a07)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongGet = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a08)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongGetandadd = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a09)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongGetandset = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a0a)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongIncrementandget = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a0b)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongGetandincrement = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a0c)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicLongMessageType AtomiclongSet = new Hazelcast.Client.Protocol.Codec.AtomicLongMessageType(unchecked((int)(0x0a0d)));

		private readonly int id;

		internal AtomicLongMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.AtomicLongMessageType.id;
		}
	}
}
