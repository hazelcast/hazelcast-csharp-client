using System;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnQueueProxy<E> : ClientTxnProxy, ITransactionalQueue<E>
    {
        public ClientTxnQueueProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        public virtual bool Offer(E e)
        {
            try
            {
                return Offer(e, 0, TimeUnit.MILLISECONDS);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool Offer(E e, long timeout, TimeUnit unit)
        {
            var data = ToData(e);
            var request = TransactionalQueueOfferCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), data, 
                unit.ToMillis(timeout));
            return Invoke(request, m => TransactionalQueueOfferCodec.DecodeResponse(m).response);
        }

        public virtual E Poll()
        {
            try
            {
                return Poll(0, TimeUnit.MILLISECONDS);
            }
            catch (Exception)
            {
                return default(E);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual E Poll(long timeout, TimeUnit unit)
        {
            var request = TransactionalQueuePollCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                unit.ToMillis(timeout));
            var result = Invoke(request, m => TransactionalQueuePollCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
        }

        public virtual E Peek()
        {
            try
            {
                return Peek(0, TimeUnit.MILLISECONDS);
            }
            catch (Exception)
            {
                return default(E);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual E Peek(long timeout, TimeUnit unit)
        {
            var request = TransactionalQueuePeekCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                unit.ToMillis(timeout));
            var result = Invoke(request, m => TransactionalQueuePeekCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
        }

        public virtual int Size()
        {
            var request = TransactionalQueueSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalQueueSizeCodec.DecodeResponse(m).response);
        }

        public override string GetServiceName()
        {
            return ServiceNames.Queue;
        }

        internal override void OnDestroy()
        {
        }
    }
}