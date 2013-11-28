using System;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Serialization.Hook
{
	
	public sealed class MapDataSerializerHook : DataSerializerHook
	{
		public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MapDsFactory, -10);
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

		public int GetFactoryId()
		{
			return FId;
		}

		public IDataSerializableFactory CreateFactory()
		{
            var constructors = new Func<int, IIdentifiedDataSerializable>[Len];
            constructors[Put] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");}; 
            constructors[Get] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");}; 
            constructors[Remove] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");}; 
            constructors[PutBackup] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");};
            constructors[RemoveBackup] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");};
            constructors[KeySet] = delegate(int i){return new MapKeySet();};
            constructors[Values] = delegate(int i){return new MapValueCollection();};
            constructors[EntrySet] = delegate(int i){return new MapEntrySet();}; 
            constructors[EntryView] = delegate(int i){return new SimpleEntryView<object,object>();};
            constructors[MapStats] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");};
            constructors[QueryResultEntry] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");};
            constructors[QueryResultSet] = delegate(int i){throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");};
			return new ArrayDataSerializableFactory(constructors);
		}

	}
}
