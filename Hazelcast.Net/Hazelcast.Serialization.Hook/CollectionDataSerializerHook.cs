using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class CollectionDataSerializerHook : DataSerializerHook
    {
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.CollectionDsFactory, -20);
        public static readonly int CollectionAdd = 1;
        public static readonly int CollectionAddBackup = 2;
        public static readonly int ListAdd = 3;
        public static readonly int ListGet = 4;
        public static readonly int CollectionRemove = 5;
        public static readonly int CollectionRemoveBackup = 6;
        public static readonly int CollectionSize = 7;
        public static readonly int CollectionClear = 8;
        public static readonly int CollectionClearBackup = 9;
        public static readonly int ListSet = 10;
        public static readonly int ListSetBackup = 11;
        public static readonly int ListRemove = 12;
        public static readonly int ListIndexOf = 13;
        public static readonly int CollectionContains = 14;
        public static readonly int CollectionAddAll = 15;
        public static readonly int CollectionAddAllBackup = 16;
        public static readonly int ListAddAll = 17;
        public static readonly int ListSub = 18;
        public static readonly int CollectionCompareAndRemove = 19;
        public static readonly int CollectionGetAll = 20;
        public static readonly int CollectionEventFilter = 21;
        public static readonly int CollectionEvent = 22;
        public static readonly int CollectionItem = 23;
        public static readonly int CollectionReserveAdd = 24;
        public static readonly int CollectionReserveRemove = 25;
        public static readonly int CollectionTxnAdd = 26;
        public static readonly int CollectionTxnAddBackup = 27;
        public static readonly int CollectionTxnRemove = 28;
        public static readonly int CollectionTxnRemoveBackup = 29;
        public static readonly int CollectionPrepare = 30;
        public static readonly int CollectionPrepareBackup = 31;
        public static readonly int CollectionRollback = 32;
        public static readonly int CollectionRollbackBackup = 33;
        public static readonly int TxCollectionItem = 34;
        public static readonly int TxRollback = 35;
        public static readonly int ListReplication = 36;
        public static readonly int SetReplication = 37;

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[SetReplication + 1];
            constructors[CollectionAdd] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionAddBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListAdd] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListGet] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionRemove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionRemoveBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionSize] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionClear] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionClearBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListSet] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListSetBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListRemove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListIndexOf] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionContains] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionAddAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionAddAllBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListAddAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListSub] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionCompareAndRemove] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionGetAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionEventFilter] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new CollectionEventFilter();
            constructors[CollectionEvent] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new CollectionEvent();
            constructors[CollectionItem] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new CollectionItem();
            constructors[CollectionReserveAdd] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionReserveRemove] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionTxnAdd] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionTxnAddBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionTxnRemove] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionTxnRemoveBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionPrepare] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionPrepareBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionRollback] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CollectionRollbackBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxCollectionItem] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxRollback] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ListReplication] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[SetReplication] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}