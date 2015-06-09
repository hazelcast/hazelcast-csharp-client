using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class ClientMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientAuthentication = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x2)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientAuthenticationcustom = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x3)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientMembershiplistener = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x4)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientCreateproxy = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x5)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientDestroyproxy = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x6)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientGetpartitions = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x8)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientRemovealllisteners = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0x9)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientAddpartitionlostlistener = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xa)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientRemovepartitionlostlistener = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xb)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientGetdistributedobject = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xc)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientAdddistributedobjectlistener = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xd)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientRemovedistributedobjectlistener = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xe)));

		public static readonly Hazelcast.Client.Protocol.Codec.ClientMessageType ClientPing = new Hazelcast.Client.Protocol.Codec.ClientMessageType(unchecked((int)(0xf)));

		private readonly int id;

		internal ClientMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return id;
		}
	}
}
