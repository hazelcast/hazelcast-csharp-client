using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public class DistributedObjectListenerRequest : IPortable
    {
        public virtual int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public virtual int GetClassId()
        {
            return ClientPortableHook.Listener;
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