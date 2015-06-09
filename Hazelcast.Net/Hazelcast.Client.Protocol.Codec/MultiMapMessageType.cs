using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class MultiMapMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapPut = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0201)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapGet = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0202)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapRemove = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0203)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapKeyset = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0204)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapValues = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0205)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapEntryset = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0206)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapContainskey = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0207)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapContainsvalue = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0208)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapContainsentry = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0209)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapSize = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020a)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapClear = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020b)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapCount = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020c)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapAddentrylistenertokey = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020d)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapAddentrylistener = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020e)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapRemoveentrylistener = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x020f)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapLock = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0210)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapTrylock = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0211)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapIslocked = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0212)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapUnlock = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0213)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapForceunlock = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0214)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapRemoveentry = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0215)));

		public static readonly Hazelcast.Client.Protocol.Codec.MultiMapMessageType MultimapValuecount = new Hazelcast.Client.Protocol.Codec.MultiMapMessageType(unchecked((int)(0x0216)));

		private readonly int id;

		internal MultiMapMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.MultiMapMessageType.id;
		}
	}
}
