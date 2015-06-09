using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class ExecutorServiceMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceShutdown = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0901)));

		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceIsshutdown = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0902)));

		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceCancelonpartition = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0903)));

		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceCancelonaddress = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0904)));

		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceSubmittopartition = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0905)));

		public static readonly Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType ExecutorserviceSubmittoaddress = new Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType(unchecked((int)(0x0906)));

		private readonly int id;

		internal ExecutorServiceMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.ExecutorServiceMessageType.id;
		}
	}
}
