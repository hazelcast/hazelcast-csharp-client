using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListRemoveRequest : CollectionRequest
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

    }
}