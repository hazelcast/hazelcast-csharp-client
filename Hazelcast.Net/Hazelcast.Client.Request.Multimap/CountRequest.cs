using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class CountRequest : MultiMapKeyBasedRequest, IRetryableRequest
    {

        public CountRequest(string name, Data key) : base(name, key)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Count;
        }
    }
}