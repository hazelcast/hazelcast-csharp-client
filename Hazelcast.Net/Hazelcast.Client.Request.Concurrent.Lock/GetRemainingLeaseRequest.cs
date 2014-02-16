using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public sealed class GetRemainingLeaseRequest : ClientRequest
    {
        private Data key;

        public GetRemainingLeaseRequest()
        {
        }

        public GetRemainingLeaseRequest(Data key)
        {
            this.key = key;
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
        public override void WritePortable(IPortableWriter writer)
        {
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

    }
}