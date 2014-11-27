using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListAddRequest : CollectionAddRequest
    {
        private readonly int index;

        public ListAddRequest(string name, IData value, int index) : base(name, value)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListAdd;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            base.Write(writer);
        }
    }
}