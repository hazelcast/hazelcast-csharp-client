using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapRemoveInterceptorRequest : ClientRequest
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

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.RemoveInterceptor;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("id", id);
        }

    }
}