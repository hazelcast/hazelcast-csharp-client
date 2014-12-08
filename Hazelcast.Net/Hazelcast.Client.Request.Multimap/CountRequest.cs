using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class CountRequest : MultiMapKeyBasedRequest, IRetryableRequest
    {
        private readonly long threadId;

        public CountRequest(string name, IData key, long threadId)
            : base(name, key)
        {
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Count;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("threadId", threadId);
            base.Write(writer);
        }
    }
}