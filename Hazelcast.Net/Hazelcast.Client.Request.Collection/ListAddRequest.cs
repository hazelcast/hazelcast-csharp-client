using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class ListAddRequest : CollectionAddRequest
    {
        private int index;

        public ListAddRequest()
        {
        }

        public ListAddRequest(string name, Data value, int index) : base(name, value)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListAdd;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            base.WritePortable(writer);
        }

    }
}