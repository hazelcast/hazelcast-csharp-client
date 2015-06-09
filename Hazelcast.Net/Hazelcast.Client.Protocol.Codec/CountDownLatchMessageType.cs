using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class CountDownLatchMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType CountdownlatchAwait = new Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType(unchecked((int)(0x0c01)));

		public static readonly Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType CountdownlatchCountdown = new Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType(unchecked((int)(0x0c02)));

		public static readonly Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType CountdownlatchGetcount = new Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType(unchecked((int)(0x0c03)));

		public static readonly Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType CountdownlatchTrysetcount = new Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType(unchecked((int)(0x0c04)));

		private readonly int id;

		internal CountDownLatchMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.CountDownLatchMessageType.id;
		}
	}
}
