using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal sealed class GetRemainingLeaseRequest : ClientRequest
    {
        private readonly IData key;

        public GetRemainingLeaseRequest(IData key)
        {
            this.key = key;
        }

        public string GetServiceName()
        {
            return ServiceNames.Lock;
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.GetRemainingLease;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}