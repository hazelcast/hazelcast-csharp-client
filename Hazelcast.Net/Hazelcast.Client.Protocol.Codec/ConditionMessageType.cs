using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class ConditionMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.ConditionMessageType ConditionAwait = new Hazelcast.Client.Protocol.Codec.ConditionMessageType(unchecked((int)(0x0801)));

		public static readonly Hazelcast.Client.Protocol.Codec.ConditionMessageType ConditionBeforeawait = new Hazelcast.Client.Protocol.Codec.ConditionMessageType(unchecked((int)(0x0802)));

		public static readonly Hazelcast.Client.Protocol.Codec.ConditionMessageType ConditionSignal = new Hazelcast.Client.Protocol.Codec.ConditionMessageType(unchecked((int)(0x0803)));

		public static readonly Hazelcast.Client.Protocol.Codec.ConditionMessageType ConditionSignalall = new Hazelcast.Client.Protocol.Codec.ConditionMessageType(unchecked((int)(0x0804)));

		private readonly int id;

		internal ConditionMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.ConditionMessageType.id;
		}
	}
}
