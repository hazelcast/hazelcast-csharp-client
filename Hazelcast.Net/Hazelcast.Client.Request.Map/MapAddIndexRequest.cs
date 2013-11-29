using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapAddIndexRequest : IPortable
    {
        private string attribute;
        private string name;

        private bool ordered;

        public MapAddIndexRequest()
        {
        }

        public MapAddIndexRequest(string name, string attribute, bool ordered)
        {
            this.name = name;
            this.attribute = attribute;
            this.ordered = ordered;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.AddIndex;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("a", attribute);
            writer.WriteBoolean("o", ordered);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            attribute = reader.ReadUTF("a");
            ordered = reader.ReadBoolean("o");
        }
    }
}