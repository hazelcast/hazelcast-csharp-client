using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    [Serializable]
    internal sealed class AddMembershipListenerRequest : ClientRequest, IRetryableRequest
    {

        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.MembershipListener;
        }

        public override void WritePortable(IPortableWriter writer)
        {
        }

    }
}