using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Map;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class MultiMapPortableHook : IPortableHook
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
        public const int RemoveEntryListener = 24;
        public const int TxnMmRemoveAll = 25;

        public const int TotalSize = TxnMmRemoveAll+1;

        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MultimapPortableFactory, -12);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[TotalSize];
            constructors[EntrySetResponse] = arg => new PortableEntrySetResponse();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}