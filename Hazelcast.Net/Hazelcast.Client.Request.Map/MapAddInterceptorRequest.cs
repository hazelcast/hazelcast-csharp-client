using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapAddInterceptorRequest : ClientRequest
    {
        private IMapInterceptor mapInterceptor;
        private string name;

        public MapAddInterceptorRequest()
        {
        }

        public MapAddInterceptorRequest(string name, IMapInterceptor mapInterceptor)
        {
            this.name = name;
            this.mapInterceptor = mapInterceptor;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.AddInterceptor;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteObject(mapInterceptor);
        }

    }
}