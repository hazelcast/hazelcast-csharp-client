using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapAddIndexRequest : ClientRequest
    {
        private string attribute;
        private string name;

        private bool ordered;

        public MapAddIndexRequest(string name, string attribute, bool ordered)
        {
            this.name = name;
            this.attribute = attribute;
            this.ordered = ordered;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.AddIndex;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("a", attribute);
            writer.WriteBoolean("o", ordered);
        }

    }
}