using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapAddInterceptorRequest : IPortable
    {
        private MapInterceptor mapInterceptor;
        private string name;

        public MapAddInterceptorRequest()
        {
        }

        public MapAddInterceptorRequest(string name, MapInterceptor mapInterceptor)
        {
            this.name = name;
            this.mapInterceptor = mapInterceptor;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.AddInterceptor;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteObject(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            IObjectDataInput input = reader.GetRawDataInput();
            mapInterceptor = input.ReadObject<MapInterceptor>();
        }
    }
}