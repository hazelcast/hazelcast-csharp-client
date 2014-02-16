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

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IDataSerializableFactory CreateFactory()
        {
            return null;
        }
    }
}