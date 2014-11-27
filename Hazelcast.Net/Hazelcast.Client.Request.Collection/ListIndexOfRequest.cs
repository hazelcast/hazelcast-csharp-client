using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListIndexOfRequest : CollectionRequest
    {
        internal bool last;
        internal IData value;

        public ListIndexOfRequest(string name, IData value, bool last) : base(name)
        {
            this.value = value;
            this.last = last;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListIndexOf;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteBoolean("l", last);
            writer.GetRawDataOutput().WriteData(value);
        }
    }
}