using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class ValuesRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {
        public ValuesRequest()
        {
        }

        public ValuesRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Values;
        }
    }
}