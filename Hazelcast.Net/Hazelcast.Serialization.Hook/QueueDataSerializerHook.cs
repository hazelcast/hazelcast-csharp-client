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

        //import com.hazelcast.queue.tx.QueueTransactionRollbackOperation;
        //import com.hazelcast.queue.tx.TxnOfferBackupOperation;
        //import com.hazelcast.queue.tx.TxnOfferOperation;
        //import com.hazelcast.queue.tx.TxnPeekOperation;
        //import com.hazelcast.queue.tx.TxnPollBackupOperation;
        //import com.hazelcast.queue.tx.TxnPollOperation;
        //import com.hazelcast.queue.tx.TxnPrepareBackupOperation;
        //import com.hazelcast.queue.tx.TxnPrepareOperation;
        //import com.hazelcast.queue.tx.TxnReserveOfferOperation;
        //import com.hazelcast.queue.tx.TxnReservePollOperation;
        //import com.hazelcast.queue.tx.TxnRollbackBackupOperation;
        //import com.hazelcast.queue.tx.TxnRollbackOperation;
        //    static final int EMPTY_ID = 21;
        public int GetFactoryId()
        {
            return FId;
        }

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[TxnPeek + 1];
            constructors[Offer] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[OfferBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Poll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[PollBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Peek] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[AddAllBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[AddAll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ClearBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Clear] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CompareAndRemoveBackup] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CompareAndRemove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Contains] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[DrainBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Drain] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Iterator] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[QueueEvent] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new QueueEvent();
            constructors[QueueEventFilter] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new QueueEventFilter();
            constructors[QueueItem] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new QueueItem();
            constructors[QueueReplication] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[RemoveBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Remove] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Size] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnOfferBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnOffer] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPollBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPoll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPrepareBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnPrepare] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnReserveOffer] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnReservePoll] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRollbackBackup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxnRollback] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[CheckEvict] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[QueueContainer] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new QueueContainer(null);
            constructors[TransactionRollback] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[TxQueueItem] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new TxQueueItem();
            constructors[TxnPeek] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}