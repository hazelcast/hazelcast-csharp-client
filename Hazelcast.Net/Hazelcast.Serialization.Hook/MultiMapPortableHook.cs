using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class MultiMapPortableHook : IPortableHook
    {
        public const int Clear = 1;
        public const int ContainsEntry = 2;
        public const int Count = 3;
        public const int EntrySet = 4;
        public const int GetAll = 5;
        public const int Get = 6;
        public const int KeySet = 7;
        public const int Put = 8;
        public const int RemoveAll = 9;
        public const int Remove = 10;
        public const int Set = 11;
        public const int Size = 12;
        public const int Values = 13;
        public const int AddEntryListener = 14;
        public const int EntrySetResponse = 15;
        public const int Lock = 16;
        public const int Unlock = 17;
        public const int IsLocked = 18;
        public const int TxnMmPut = 19;
        public const int TxnMmGet = 20;
        public const int TxnMmRemove = 21;
        public const int TxnMmValueCount = 22;
        public const int TxnMmSize = 23;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MultimapPortableFactory, -12);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[TxnMmSize + 1];
            constructors[Clear] = arg => new ClearRequest();
            constructors[ContainsEntry] = arg => new ContainsEntryRequest();
            constructors[Count] = arg => new CountRequest();
            constructors[EntrySet] = arg => new EntrySetRequest();
            constructors[GetAll] = arg => new GetAllRequest();
            constructors[KeySet] = arg => new KeySetRequest();
            constructors[Put] = arg => new PutRequest();
            constructors[RemoveAll] = arg => new RemoveAllRequest();
            constructors[Remove] = arg => new RemoveRequest();
            constructors[Size] = arg => new SizeRequest();
            constructors[Values] = arg => new ValuesRequest();
            constructors[AddEntryListener] = arg => new AddEntryListenerRequest();
            constructors[EntrySetResponse] = arg => new PortableEntrySetResponse();
            constructors[Lock] = arg => new MultiMapLockRequest();
            constructors[Unlock] = arg => new MultiMapUnlockRequest();
            constructors[IsLocked] = arg => new MultiMapIsLockedRequest();
            constructors[TxnMmPut] = arg => new TxnMultiMapPutRequest();
            constructors[TxnMmGet] = arg => new TxnMultiMapGetRequest();
            constructors[TxnMmRemove] = arg => new TxnMultiMapRemoveRequest();
            constructors[TxnMmValueCount] = arg => new TxnMultiMapValueCountRequest();
            constructors[TxnMmSize] = arg => new TxnMultiMapSizeRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}