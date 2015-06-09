using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class SetMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetSize = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0601)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetContains = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0602)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetContainsall = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0603)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetAdd = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0604)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetRemove = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0605)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetAddall = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0606)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetCompareandremoveall = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0607)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetCompareandretainall = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0608)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetClear = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x0609)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetGetall = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x060a)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetAddlistener = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x060b)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetRemovelistener = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x060c)));

		public static readonly Hazelcast.Client.Protocol.Codec.SetMessageType SetIsempty = new Hazelcast.Client.Protocol.Codec.SetMessageType(unchecked((int)(0x060d)));

		private readonly int id;

		internal SetMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.SetMessageType.id;
		}
	}
}
