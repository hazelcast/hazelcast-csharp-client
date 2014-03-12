using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class SizeRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {

        public SizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Size;
        }
    }
}