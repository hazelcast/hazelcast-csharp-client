using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class GetAllRequest : MultiMapKeyBasedRequest, IRetryableRequest
    {
        private readonly long threadId;

        public GetAllRequest(string name, IData key, long threadId)
            : base(name, key)
        {
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.GetAll;
        }


        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("threadId", threadId);
            base.Write(writer);
        }
    }
}