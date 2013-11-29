using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class GetAllRequest : MultiMapKeyBasedRequest, IRetryableRequest
    {
        public GetAllRequest()
        {
        }

        public GetAllRequest(string name, Data key) : base(name, key)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.GetAll;
        }
    }
}