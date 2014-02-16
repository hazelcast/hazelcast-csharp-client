using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    public class ClientSetProxy<E> : AbstractClientCollectionProxy<E>, IHSet<E>
    {
        public ClientSetProxy(string serviceName, string name) : base(serviceName, name)
        {
        }
    }
}