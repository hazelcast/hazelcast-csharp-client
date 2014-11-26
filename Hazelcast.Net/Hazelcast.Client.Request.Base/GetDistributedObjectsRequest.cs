using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    internal class GetDistributedObjectsRequest : ClientRequest
    {
        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.GetDistributedObjectInfo;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
        }


    }
}