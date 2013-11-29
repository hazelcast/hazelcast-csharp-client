using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public class GetDistributedObjectsRequest : IPortable
    {
        public virtual int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public virtual int GetClassId()
        {
            return ClientPortableHook.GetDistributedObjectInfo;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
        }
    }
}