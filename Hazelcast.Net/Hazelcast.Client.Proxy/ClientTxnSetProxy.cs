using Hazelcast.Client.Request.Collection;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    public class ClientTxnSetProxy<E> : AbstractClientTxnCollectionProxy<E>, ITransactionalSet<E>
    {
        public ClientTxnSetProxy(string name, TransactionContextProxy proxy)
            : base(name, proxy)
        {
        }

        public virtual bool Add(E e)
        {
            ThrowExceptionIfNull(e);
            Data value = ToData(e);
            var request = new TxnSetAddRequest(GetName(), value);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual bool Remove(E e)
        {
            ThrowExceptionIfNull(e);
            Data value = ToData(e);
            var request = new TxnSetRemoveRequest(GetName(), value);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual int Size()
        {
            var request = new TxnSetSizeRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }

        public override string GetServiceName()
        {
            return ServiceNames.Set;
        }
    }
}