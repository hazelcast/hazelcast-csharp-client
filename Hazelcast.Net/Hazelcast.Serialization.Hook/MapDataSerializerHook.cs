using System;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Util;

namespace Hazelcast.Serialization.Hook
{
    public sealed class MapDataSerializerHook : DataSerializerHook
    {
        public const int Put = 0;
        public const int Get = 1;
        public const int Remove = 2;
        public const int PutBackup = 3;
        public const int RemoveBackup = 4;
        public const int KeySet = 8;
        public const int Values = 9;
        public const int EntrySet = 10;
        public const int EntryView = 11;
        public const int MapStats = 12;
        public const int QueryResultEntry = 13;
        public const int QueryResultSet = 14;
        private const int Len = QueryResultSet + 1;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MapDsFactory, -10);

        public int GetFactoryId()
        {
            return FId;
        }

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[Len];
            constructors[KeySet] = delegate { return new MapKeySet(); };
            constructors[Values] = delegate { return new MapValueCollection(); };
            constructors[EntrySet] = delegate { return new MapEntrySet(); };
            constructors[EntryView] = delegate { return new SimpleEntryView<object, object>(); };
            constructors[QueryResultEntry] = delegate { return new QueryResultEntry();};
            constructors[QueryResultSet] = delegate { return new QueryResultSet(); };
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}