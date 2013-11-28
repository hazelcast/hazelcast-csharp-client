using System;
using Hazelcast.Client.Spi;


namespace Hazelcast.Client.Spi
{

    public delegate ClientProxy ClientProxyFactory(Type type,string id);

    public interface IClientProxyFactory
    {
        ClientProxy Create(string id);
    }
}
