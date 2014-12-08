using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class KeySetRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {
        public KeySetRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.KeySet;
        }
    }
}