using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class RemoveAllRequest : MultiMapKeyBasedRequest
    {
        internal long threadId = -1;

        public RemoveAllRequest()
        {
        }

        public RemoveAllRequest(string name, Data key, long threadId) : base(name, key)
        {
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.RemoveAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("t", threadId);
            base.WritePortable(writer);
        }

    }
}