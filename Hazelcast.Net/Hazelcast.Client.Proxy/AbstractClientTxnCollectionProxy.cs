using System;
using Hazelcast.Client.Request.Transaction;

namespace Hazelcast.Client.Proxy
{
    internal abstract class AbstractClientTxnCollectionProxy<E> : ClientTxnProxy
    {
        protected internal AbstractClientTxnCollectionProxy(string name, TransactionContextProxy proxy)
            : base(name, proxy)
        {
        }

        internal override void OnDestroy()
        {
        }

        public override string GetName()
        {
            return (string) GetId();
        }

        protected internal virtual void ThrowExceptionIfNull(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("Object is null");
            }
        }
    }
}