using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionAddRequest : CollectionRequest
    {
        protected internal IData value;

        public CollectionAddRequest(string name, IData value) : base(name)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionAdd;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.GetRawDataOutput().WriteData(value);
        }
    }
}