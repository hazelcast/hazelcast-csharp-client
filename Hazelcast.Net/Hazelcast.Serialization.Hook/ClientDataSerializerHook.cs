using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal sealed class ClientDataSerializerHook : DataSerializerHook
    {
        public static readonly int Id = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ClientDsFactory, -3);

        public static readonly int ClientResponse = 1;

        public int GetFactoryId()
        {
            return Id;
        }

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[ClientResponse + 1];
            constructors[ClientResponse] = delegate { return new ClientResponse(); };
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}