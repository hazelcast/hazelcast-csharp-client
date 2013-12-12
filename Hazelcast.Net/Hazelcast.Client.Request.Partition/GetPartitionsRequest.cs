using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Partition
{
    [Serializable]
    public sealed class GetPartitionsRequest : IIdentifiedDataSerializable, IRetryableRequest
    {
        public int GetFactoryId()
        {
            return PartitionDataSerializerHook.FId;
        }

        public int GetId()
        {
            return PartitionDataSerializerHook.GetPartitions;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
        }

        public string GetJavaClassName()
        {
            throw new NotImplementedException();
        }
    }
}