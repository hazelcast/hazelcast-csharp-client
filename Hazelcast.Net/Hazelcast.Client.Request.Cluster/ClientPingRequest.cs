using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    internal sealed class ClientPingRequest : ClientRequest
    {
        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.ClientPing;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
        }
    }
}