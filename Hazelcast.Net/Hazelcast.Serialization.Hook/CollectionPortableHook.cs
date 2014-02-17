using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class CollectionPortableHook : IPortableHook
    {
        public const int CollectionSize = 1;
        public const int CollectionContains = 2;
        public const int CollectionAdd = 3;
        public const int CollectionRemove = 4;
        public const int CollectionAddAll = 5;
        public const int CollectionCompareAndRemove = 6;
        public const int CollectionClear = 7;
        public const int CollectionGetAll = 8;
        public const int CollectionAddListener = 9;
        public const int ListAddAll = 10;
        public const int ListGet = 11;
        public const int ListSet = 12;
        public const int ListAdd = 13;
        public const int ListRemove = 14;
        public const int ListIndexOf = 15;
        public const int ListSub = 16;
        public const int TxnListAdd = 17;
        public const int TxnListRemove = 18;
        public const int TxnListSize = 19;
        public const int TxnSetAdd = 20;
        public const int TxnSetRemove = 21;
        public const int TxnSetSize = 22;
        public const int CollectionRemoveListener = 23;

        private const int Size = CollectionRemoveListener + 1;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.CollectionPortableFactory, -20);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            return new ArrayPortableFactory();
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}