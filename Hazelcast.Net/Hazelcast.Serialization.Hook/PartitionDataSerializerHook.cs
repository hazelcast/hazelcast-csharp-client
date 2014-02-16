using System;
using Hazelcast.Client.Request.Partition;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class PartitionDataSerializerHook : DataSerializerHook
    {
        public const int Partitions = 2;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.PartitionDsFactory, -2);

        public int GetFactoryId()
        {
            return FId;
        }

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[Partitions + 1];
            constructors[Partitions] = arg => new PartitionsResponse();
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}