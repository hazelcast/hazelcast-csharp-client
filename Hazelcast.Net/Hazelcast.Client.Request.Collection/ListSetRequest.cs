using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListSetRequest : CollectionRequest
    {
        private readonly int index;
        private readonly IData value;

        public ListSetRequest(string name, int index, IData value) : base(name)
        {
            this.index = index;
            this.value = value;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListSet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteInt("i", index);
            writer.GetRawDataOutput().WriteData(value);
        }
    }
}