using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapRemoveInterceptorRequest : IPortable
    {
        private string id;
        private string name;

        public MapRemoveInterceptorRequest()
        {
        }

        public MapRemoveInterceptorRequest(string name, string id)
        {
            this.name = name;
            this.id = id;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.RemoveInterceptor;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("id", id);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            id = reader.ReadUTF("id");
        }
    }
}