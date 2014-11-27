using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListRemoveRequest : CollectionRequest
    {
        internal int index;

        public ListRemoveRequest(string name, int index) : base(name)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteInt("i", index);
        }
    }
}