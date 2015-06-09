using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class AtomicReferenceMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceApply = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b01)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceAlter = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b02)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceAlterandget = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b03)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceGetandalter = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b04)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceContains = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b05)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceCompareandset = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b06)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceGet = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b08)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceSet = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b09)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceClear = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b0a)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceGetandset = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b0b)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceSetandget = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b0c)));

		public static readonly Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType AtomicreferenceIsnull = new Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType(unchecked((int)(0x0b0d)));

		private readonly int id;

		internal AtomicReferenceMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.AtomicReferenceMessageType.id;
		}
	}
}
