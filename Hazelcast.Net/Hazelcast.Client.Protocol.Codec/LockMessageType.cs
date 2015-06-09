using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class LockMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockIslocked = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0701)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockIslockedbycurrentthread = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0702)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockGetlockcount = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0703)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockGetremainingleasetime = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0704)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockLock = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0705)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockUnlock = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0706)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockForceunlock = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0707)));

		public static readonly Hazelcast.Client.Protocol.Codec.LockMessageType LockTrylock = new Hazelcast.Client.Protocol.Codec.LockMessageType(unchecked((int)(0x0708)));

		private readonly int id;

		internal LockMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.LockMessageType.id;
		}
	}
}
