using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class CollectionPortableHook : IPortableHook
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
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.CollectionPortableFactory, -20);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[TxnSetSize + 1];
            constructors[CollectionSize] = arg => new CollectionSizeRequest();
            constructors[CollectionContains] = arg => new CollectionContainsRequest();
            constructors[CollectionAdd] = arg => new CollectionAddRequest();
            constructors[CollectionRemove] = arg => new CollectionRemoveRequest();
            constructors[CollectionAddAll] = arg => new CollectionAddAllRequest();
            constructors[CollectionCompareAndRemove] = arg => new CollectionCompareAndRemoveRequest();
            constructors[CollectionClear] = arg => new CollectionClearRequest();
            constructors[CollectionGetAll] = arg => new CollectionGetAllRequest();
            constructors[CollectionAddListener] = arg => new CollectionAddListenerRequest();
            constructors[ListAddAll] = arg => new ListAddAllRequest();
            constructors[ListGet] = arg => new ListGetRequest();
            constructors[ListSet] = arg => new ListSetRequest();
            constructors[ListAdd] = arg => new ListAddRequest();
            constructors[ListRemove] = arg => new ListRemoveRequest();
            constructors[ListIndexOf] = arg => new ListIndexOfRequest();
            constructors[ListSub] = arg => new ListSubRequest();
            constructors[TxnListAdd] = arg => new TxnListAddRequest();
            constructors[TxnListRemove] = arg => new TxnListRemoveRequest();
            constructors[TxnListSize] = arg => new TxnListSizeRequest();
            constructors[TxnSetAdd] = arg => new TxnSetAddRequest();
            constructors[TxnSetRemove] = arg => new TxnSetRemoveRequest();
            constructors[TxnSetSize] = arg => new TxnSetSizeRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}