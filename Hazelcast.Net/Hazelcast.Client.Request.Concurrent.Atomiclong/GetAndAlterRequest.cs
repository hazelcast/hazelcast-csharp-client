using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal class GetAndAlterRequest : AbstractAlterRequest
    {
        public GetAndAlterRequest(string name, IData function) : base(name, function)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.GetAndAlter;
        }
    }
}