using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnSetProxy<E> : AbstractClientTxnCollectionProxy<E>, ITransactionalSet<E>
    {
        public ClientTxnSetProxy(string name, TransactionContextProxy proxy)
            : base(name, proxy)
        {
        }

        public virtual bool Add(E e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalSetAddCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalSetAddCodec.DecodeResponse(m).response);
        }

        public virtual bool Remove(E e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalSetRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalSetRemoveCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = TransactionalSetSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalSetSizeCodec.DecodeResponse(m).response);
        }

        public override string GetServiceName()
        {
            return ServiceNames.Set;
        }
    }
}