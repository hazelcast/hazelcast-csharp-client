using System;
using System.IO;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client.Request.Transaction
{
    internal sealed class TransactionProxy
    {
        [ThreadStatic]
        private static bool? _threadFlag ;

        private readonly TransactionOptions options;
        private readonly long threadId = ThreadUtil.GetThreadId();

        private readonly HazelcastClient client;

        private Address txOwner;

        private long startTime;
        private TransactionState state = TransactionState.NoTxn;
        private string txnId;

        internal TransactionProxy(HazelcastClient client, TransactionOptions options, Address txOwner)
        {
            this.options = options;
            this.client =  client;
            this.txOwner = txOwner;
        }

        public string GetTxnId()
        {
            return txnId;
        }

        public TransactionState GetState()
        {
            return state;
        }

        public long GetTimeoutMillis()
        {
            return options.GetTimeoutMillis();
        }

        internal void Begin()
        {
            try
            {
                if (state == TransactionState.Active)
                {
                    throw new InvalidOperationException("Transaction is already active");
                }
                CheckThread();
                if (_threadFlag != null)
                {
                    throw new InvalidOperationException("Nested transactions are not allowed!");
                }
                _threadFlag = true;
                startTime = Clock.CurrentTimeMillis();
                txnId = Invoke<string>(new CreateTransactionRequest(options));
                state = TransactionState.Active;
            }
            catch (Exception e)
            {
                _threadFlag = null;
                throw ExceptionUtil.Rethrow(e);
            }
        }

        internal void Commit()
        {
            try
            {
                if (state != TransactionState.Active)
                {
                    throw new TransactionNotActiveException("Transaction is not active");
                }
                CheckThread();
                CheckTimeout();
                Invoke<object>(new CommitTransactionRequest());
                state = TransactionState.Committed;
            }
            catch (Exception e)
            {
                state = TransactionState.RollingBack;
                throw ExceptionUtil.Rethrow(e);
            }
            finally
            {
                _threadFlag = null;
            }
        }

        internal void Rollback()
        {
            try
            {
                if (state == TransactionState.NoTxn || state == TransactionState.RolledBack)
                {
                    throw new InvalidOperationException("Transaction is not active");
                }
                if (state == TransactionState.RollingBack)
                {
                    state = TransactionState.RolledBack;
                    return;
                }
                CheckThread();
                try
                {
                    Invoke<object>(new RollbackTransactionRequest());
                }
                catch (Exception)
                {
                }
                state = TransactionState.RolledBack;
            }
            finally
            {
                _threadFlag = null;
            }
        }

        internal T Invoke<T>(ClientRequest request)
        {
            var btr = request as BaseTransactionRequest;
            if (btr != null)
            {
                btr.TxnId = txnId;
                btr.ClientThreadId = threadId;
            }
            var ss = client.GetSerializationService();
            var rpc = client.GetRemotingService();
            try
            {
                var task = rpc.Send(request, txOwner);
                var result = task.Result;
                return ss.ToObject<T>(result);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        private void CheckThread()
        {
            if (threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Transaction cannot span multiple threads!");
            }
        }

        private void CheckTimeout()
        {
            if (startTime + options.GetTimeoutMillis() < Clock.CurrentTimeMillis())
            {
                throw new TransactionException("Transaction is timed-out!");
            }
        }


    }
}