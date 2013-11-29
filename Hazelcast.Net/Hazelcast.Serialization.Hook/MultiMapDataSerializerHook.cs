using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class MultiMapDataSerializerHook : DataSerializerHook
    {
        public const int AddAllBackup = 0;
        public const int AddAll = 1;
        public const int ClearBackup = 2;
        public const int Clear = 3;
        public const int CompareAndRemoveBackup = 4;
        public const int CompareAndRemove = 5;
        public const int ContainsAll = 6;
        public const int ContainsEntry = 7;
        public const int Contains = 8;
        public const int Count = 9;
        public const int EntrySet = 10;
        public const int GetAll = 11;
        public const int Get = 12;
        public const int IndexOf = 13;
        public const int KeySet = 14;
        public const int PutBackup = 15;
        public const int Put = 16;
        public const int RemoveAllBackup = 17;
        public const int RemoveAll = 18;
        public const int RemoveBackup = 19;
        public const int Remove = 20;
        public const int RemoveIndexBackup = 21;
        public const int RemoveIndex = 22;
        public const int SetBackup = 23;
        public const int Set = 24;
        public const int Size = 25;
        public const int Values = 26;
        public const int TxnCommitBackup = 27;
        public const int TxnCommit = 28;
        public const int TxnGenerateRecordId = 29;
        public const int TxnLockAndGet = 30;
        public const int TxnPrepareBackup = 31;
        public const int TxnPrepare = 32;
        public const int TxnPut = 33;
        public const int TxnPutBackup = 34;
        public const int TxnRemove = 35;
        public const int TxnRemoveBackup = 36;
        public const int TxnRemoveAll = 37;
        public const int TxnRemoveAllBackup = 38;
        public const int TxnRollback = 39;
        public const int TxnRollbackBackup = 40;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MultimapDsFactory, -12);

        //import com.hazelcast.multimap.operations.ClearBackupOperation;
        //import com.hazelcast.multimap.operations.ClearOperation;
        //import com.hazelcast.multimap.operations.ContainsEntryOperation;
        //import com.hazelcast.multimap.operations.CountOperation;
        //import com.hazelcast.multimap.operations.EntrySetOperation;
        //import com.hazelcast.multimap.operations.GetAllOperation;
        //import com.hazelcast.multimap.operations.KeySetOperation;
        //import com.hazelcast.multimap.operations.PutBackupOperation;
        //import com.hazelcast.multimap.operations.PutOperation;
        //import com.hazelcast.multimap.operations.RemoveAllBackupOperation;
        //import com.hazelcast.multimap.operations.RemoveAllOperation;
        //import com.hazelcast.multimap.operations.RemoveBackupOperation;
        //import com.hazelcast.multimap.operations.RemoveOperation;
        //import com.hazelcast.multimap.operations.SizeOperation;
        //import com.hazelcast.multimap.operations.ValuesOperation;
        //import com.hazelcast.multimap.transaction.TxnCommitBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnCommitOperation;
        //import com.hazelcast.multimap.transaction.TxnGenerateRecordIdOperation;
        //import com.hazelcast.multimap.transaction.TxnLockAndGetOperation;
        //import com.hazelcast.multimap.transaction.TxnPrepareBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnPrepareOperation;
        //import com.hazelcast.multimap.transaction.TxnPutBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnPutOperation;
        //import com.hazelcast.multimap.transaction.TxnRemoveAllBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnRemoveAllOperation;
        //import com.hazelcast.multimap.transaction.TxnRemoveBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnRemoveOperation;
        //import com.hazelcast.multimap.transaction.TxnRollbackBackupOperation;
        //import com.hazelcast.multimap.transaction.TxnRollbackOperation;
        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[TxnRollbackBackup + 1];
            constructors[ClearBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Clear] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ContainsEntry] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Count] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[EntrySet] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[GetAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[KeySet] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[PutBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Put] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[RemoveAllBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[RemoveAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[RemoveBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Remove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Size] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Values] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnCommitBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnCommit] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnGenerateRecordId] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnLockAndGet] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPrepareBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPrepare] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPut] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPutBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRemove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRemoveBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRemoveAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRemoveAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRollbackBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRollback] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}