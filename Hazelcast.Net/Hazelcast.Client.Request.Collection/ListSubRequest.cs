using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListSubRequest : CollectionRequest
    {
        private readonly int from;
        private readonly int to;

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
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteInt("f", from);
            writer.WriteInt("t", to);
        }
    }
}