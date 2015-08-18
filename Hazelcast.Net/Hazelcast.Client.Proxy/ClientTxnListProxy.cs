using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnListProxy<E> : AbstractClientTxnCollectionProxy<E>, ITransactionalList<E>
    {
        public ClientTxnListProxy(string name, TransactionContextProxy proxy)
            : base(name, proxy)
        {
        }

        public override string GetServiceName()
        {
            return ServiceNames.List;
        }

        public virtual bool Add(E e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalListAddCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalListAddCodec.DecodeResponse(m).response);
        }

        public virtual bool Remove(E e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalListRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalListRemoveCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = TransactionalListSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalListSizeCodec.DecodeResponse(m).response);
        }
    }
}