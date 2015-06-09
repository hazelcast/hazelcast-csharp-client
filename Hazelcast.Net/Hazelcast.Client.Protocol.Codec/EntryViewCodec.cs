using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class EntryViewCodec
	{
		private EntryViewCodec()
		{
		}

        public static SimpleEntryView<IData, IData> Decode(ClientMessage clientMessage)
		{
			SimpleEntryView<IData, IData> dataEntryView = new SimpleEntryView<IData, IData>();
			dataEntryView.SetKey(clientMessage.GetData());
			dataEntryView.SetValue(clientMessage.GetData());
			dataEntryView.SetCost(clientMessage.GetLong());
			dataEntryView.SetCreationTime(clientMessage.GetLong());
			dataEntryView.SetExpirationTime(clientMessage.GetLong());
			dataEntryView.SetHits(clientMessage.GetLong());
			dataEntryView.SetLastAccessTime(clientMessage.GetLong());
			dataEntryView.SetLastStoredTime(clientMessage.GetLong());
			dataEntryView.SetLastUpdateTime(clientMessage.GetLong());
			dataEntryView.SetVersion(clientMessage.GetLong());
			dataEntryView.SetEvictionCriteriaNumber(clientMessage.GetLong());
			dataEntryView.SetTtl(clientMessage.GetLong());
			return dataEntryView;
		}

		public static void Encode(SimpleEntryView<IData, IData> dataEntryView, ClientMessage clientMessage)
		{
			IData key = dataEntryView.GetKey();
			IData value = dataEntryView.GetValue();
			long cost = dataEntryView.GetCost();
			long creationTime = dataEntryView.GetCreationTime();
			long expirationTime = dataEntryView.GetExpirationTime();
			long hits = dataEntryView.GetHits();
			long lastAccessTime = dataEntryView.GetLastAccessTime();
			long lastStoredTime = dataEntryView.GetLastStoredTime();
			long lastUpdateTime = dataEntryView.GetLastUpdateTime();
			long version = dataEntryView.GetVersion();
			long ttl = dataEntryView.GetTtl();
			long evictionCriteriaNumber = dataEntryView.GetEvictionCriteriaNumber();
			clientMessage.Set(key).Set(value).Set(cost).Set(creationTime).Set(expirationTime).Set(hits).Set(lastAccessTime).Set(lastStoredTime).Set(lastUpdateTime).Set(version).Set(evictionCriteriaNumber).Set(ttl);
		}

		public static int CalculateDataSize(SimpleEntryView<IData, IData> entryView)
		{
			int dataSize = ClientMessage.HeaderSize;
			IData key = entryView.GetKey();
			IData value = entryView.GetValue();
			return dataSize + ParameterUtil.CalculateDataSize(key) + ParameterUtil.CalculateDataSize(value) + Bits.LongSizeInBytes * 10;
		}
	}
}
