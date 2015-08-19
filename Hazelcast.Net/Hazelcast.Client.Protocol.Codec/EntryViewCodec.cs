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

        public static SimpleEntryView<IData, IData> Decode(IClientMessage clientMessage)
        {
            var dataEntryView = new SimpleEntryView<IData, IData>();
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
            var key = dataEntryView.GetKey();
            var value = dataEntryView.GetValue();
            var cost = dataEntryView.GetCost();
            var creationTime = dataEntryView.GetCreationTime();
            var expirationTime = dataEntryView.GetExpirationTime();
            var hits = dataEntryView.GetHits();
            var lastAccessTime = dataEntryView.GetLastAccessTime();
            var lastStoredTime = dataEntryView.GetLastStoredTime();
            var lastUpdateTime = dataEntryView.GetLastUpdateTime();
            var version = dataEntryView.GetVersion();
            var ttl = dataEntryView.GetTtl();
            var evictionCriteriaNumber = dataEntryView.GetEvictionCriteriaNumber();
            clientMessage.Set(key)
                .Set(value)
                .Set(cost)
                .Set(creationTime)
                .Set(expirationTime)
                .Set(hits)
                .Set(lastAccessTime)
                .Set(lastStoredTime)
                .Set(lastUpdateTime)
                .Set(version)
                .Set(evictionCriteriaNumber)
                .Set(ttl);
        }

        public static int CalculateDataSize(SimpleEntryView<IData, IData> entryView)
        {
            var dataSize = ClientMessage.HeaderSize;
            var key = entryView.GetKey();
            var value = entryView.GetValue();
            return dataSize + ParameterUtil.CalculateDataSize(key) + ParameterUtil.CalculateDataSize(value) +
                   Bits.LongSizeInBytes*10;
        }
    }
}