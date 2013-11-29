using System;

namespace Hazelcast.Client.Spi
{
    public delegate ClientProxy ClientProxyFactory(Type type, string id);

    public interface IClientProxyFactory
    {
        ClientProxy Create(string id);
    }
}