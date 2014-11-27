using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionRemoveRequest : CollectionRequest
    {
        private readonly IData value;

        public CollectionRemoveRequest(string name, IData value) : base(name)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.GetRawDataOutput().WriteData(value);
        }
    }
}