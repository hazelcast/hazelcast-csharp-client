using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class ListRemoveRequest : CollectionRequest
    {
        internal int index;

        public ListRemoveRequest()
        {
        }

        public ListRemoveRequest(string name, int index) : base(name)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("i", index);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            index = reader.ReadInt("i");
        }
    }
}