using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientSetProxy<E> : AbstractClientCollectionProxy<E>, IHSet<E>
    {
        public ClientSetProxy(string serviceName, string name) : base(serviceName, name)
        {
        }
    }
}