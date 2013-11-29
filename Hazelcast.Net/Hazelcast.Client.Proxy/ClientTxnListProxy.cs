using Hazelcast.Client.Request.Collection;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    public class ClientTxnListProxy<E> : AbstractClientTxnCollectionProxy<E>, ITransactionalList<E>
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
            Data value = ToData(e);
            var request = new TxnListAddRequest(GetName(), value);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual bool Remove(E e)
        {
            ThrowExceptionIfNull(e);
            Data value = ToData(e);
            var request = new TxnListRemoveRequest(GetName(), value);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual int Size()
        {
            var request = new TxnListSizeRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }
    }
}