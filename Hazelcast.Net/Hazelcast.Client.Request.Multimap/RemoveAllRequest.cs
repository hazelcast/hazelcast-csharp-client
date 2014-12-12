using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class RemoveAllRequest : MultiMapKeyBasedRequest
    {
        internal long threadId;

        public RemoveAllRequest(string name, IData key, long threadId) : base(name, key)
        {
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.RemoveAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("t", threadId);
            base.Write(writer);
        }
    }
}