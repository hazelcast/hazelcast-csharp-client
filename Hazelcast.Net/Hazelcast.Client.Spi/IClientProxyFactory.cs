using System;

namespace Hazelcast.Client.Spi
{
    internal delegate ClientProxy ClientProxyFactory(Type type, string id);
}