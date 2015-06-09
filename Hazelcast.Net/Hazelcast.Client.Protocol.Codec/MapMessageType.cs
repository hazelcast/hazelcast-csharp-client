using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	[System.Serializable]
	internal sealed class MapMessageType
	{
		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapPut = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0101)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapGet = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0102)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemove = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0103)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapReplace = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0104)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapReplaceifsame = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0105)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapPutasync = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0106)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapGetasync = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0107)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemoveasync = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0108)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapContainskey = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0109)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapContainsvalue = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010a)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemoveifsame = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010b)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapDelete = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010c)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapFlush = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010d)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapTryremove = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010e)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapTryput = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x010f)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapPuttransient = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0110)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapPutifabsent = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0111)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapSet = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0112)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapLock = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0113)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapTrylock = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0114)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapIslocked = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0115)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapUnlock = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0116)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddinterceptor = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0117)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemoveinterceptor = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0118)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddentrylistenertokeywithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0119)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddentrylistenerwithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011a)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddentrylistenertokey = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011b)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddentrylistener = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011c)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddnearcacheentrylistener = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011d)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemoveentrylistener = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011e)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddpartitionlostlistener = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x011f)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapRemovepartitionlostlistener = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0120)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapGetentryview = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0121)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapEvict = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0122)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapEvictall = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0123)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapLoadall = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0124)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapLoadgivenkeys = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0125)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapKeyset = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0126)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapGetall = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0127)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapValues = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0128)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapEntryset = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0129)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapKeysetwithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012a)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapValueswithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012b)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapEntrieswithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012c)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapAddindex = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012d)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapSize = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012e)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapIsempty = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x012f)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapPutall = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0130)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapClear = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0131)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapExecuteonkey = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0132)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapSubmittokey = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0133)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapExecuteonallkeys = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0134)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapExecutewithpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0135)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapExecuteonkeys = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0136)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapForceunlock = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0137)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapKeysetwithpagingpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0138)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapValueswithpagingpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x0139)));

		public static readonly Hazelcast.Client.Protocol.Codec.MapMessageType MapEntrieswithpagingpredicate = new Hazelcast.Client.Protocol.Codec.MapMessageType(unchecked((int)(0x013a)));

		private readonly int id;

		internal MapMessageType(int messageType)
		{
			this.id = messageType;
		}

		public int Id()
		{
			return Hazelcast.Client.Protocol.Codec.MapMessageType.id;
		}
	}
}
