using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class QueueDataSerializerHook : DataSerializerHook
    {
        internal const int Offer = 0;
        internal const int Poll = 1;
        internal const int Peek = 2;
        internal const int OfferBackup = 3;
        internal const int PollBackup = 4;
        internal const int AddAllBackup = 5;
        internal const int AddAll = 6;
        internal const int ClearBackup = 7;
        internal const int Clear = 8;
        internal const int CompareAndRemoveBackup = 9;
        internal const int CompareAndRemove = 10;
        internal const int Contains = 11;
        internal const int DrainBackup = 12;
        internal const int Drain = 13;
        internal const int Iterator = 14;
        internal const int QueueEvent = 15;
        internal const int QueueEventFilter = 16;
        internal const int QueueItem = 17;
        internal const int QueueReplication = 18;
        internal const int RemoveBackup = 19;
        internal const int Remove = 20;
        internal const int Size = 22;
        public const int TxnOfferBackup = 23;
        public const int TxnOffer = 24;
        public const int TxnPollBackup = 25;
        public const int TxnPoll = 26;
        public const int TxnPrepareBackup = 27;
        public const int TxnPrepare = 28;
        public const int TxnReserveOffer = 29;
        public const int TxnReservePoll = 30;
        public const int TxnRollbackBackup = 31;
        public const int TxnRollback = 32;
        public const int CheckEvict = 33;
        public const int TransactionRollback = 34;
        public const int TxQueueItem = 35;
        public const int QueueContainer = 36;
        public const int TxnPeek = 37;
        internal static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.QueueDsFactory, -11);

        public int GetFactoryId()
        {
            return FId;
        }

        public IDataSerializableFactory CreateFactory()
        {
            return null;
        }
    }
}