using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionRemoveRequest : CollectionRequest
    {
        private Data value;

        public CollectionRemoveRequest()
        {
        }

        public CollectionRemoveRequest(string name, Data value) : base(name)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            value.WriteData(writer.GetRawDataOutput());
        }

    }
}