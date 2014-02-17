using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListSetRequest : CollectionRequest
    {
        private int index;

        private Data value;

        public ListSetRequest()
        {
        }

        public ListSetRequest(string name, int index, Data value) : base(name)
        {
            this.index = index;
            this.value = value;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListSet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("i", index);
            value.WriteData(writer.GetRawDataOutput());
        }

    }
}