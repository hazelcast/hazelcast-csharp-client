using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListSubRequest : CollectionRequest
    {
        private int from;

        private int to;

        public ListSubRequest()
        {
        }

        public ListSubRequest(string name, int from, int to) : base(name)
        {
            this.from = from;
            this.to = to;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListSub;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("f", from);
            writer.WriteInt("t", to);
        }

    }
}