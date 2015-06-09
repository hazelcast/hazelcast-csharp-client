using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class SemaphoreMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreInit = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d01)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreAcquire = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d02)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreAvailablepermits = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d03)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreDrainpermits = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d04)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreReducepermits = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d05)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreRelease = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d06)));

		public static readonly Hazelcast.Client.Protocol.Codec.SemaphoreMessageType SemaphoreTryacquire = new Hazelcast.Client.Protocol.Codec.SemaphoreMessageType(unchecked((int)(0x0d07)));

		private readonly int id;

		internal SemaphoreMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.SemaphoreMessageType.id;
		}
	}
}
