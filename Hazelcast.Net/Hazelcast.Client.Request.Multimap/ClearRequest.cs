using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class ClearRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {

        public ClearRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Clear;
        }
    }
}