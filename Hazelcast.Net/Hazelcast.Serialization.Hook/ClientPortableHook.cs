using System;
using System.Collections.Generic;
using Hazelcast.Client;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Client.Request.Partition;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class ClientPortableHook : IPortableHook
    {
        public const int GenericError = 1;
        public const int Auth = 2;
        public const int Principal = 3;
        public const int GetDistributedObjectInfo = 4;
        public const int DistributedObjectInfo = 6;
        public const int CreateProxy = 7;
        public const int DestroyProxy = 8;
        public const int Listener = 9;
        public const int MembershipListener = 10;
        public const int ClientPing = 11;
        public const int GetPartitions = 12;
        public const int RemoveListener = 13;

        public const int TotalSize = RemoveListener+1;

        public static readonly int Id = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ClientPortableFactory, -3);

        public virtual int GetFactoryId()
        {
            return Id;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[TotalSize];
            constructors[GenericError] = arg => new GenericError();
            //constructors[Auth] = arg => new AuthenticationRequest();
            constructors[Principal] = arg => new ClientPrincipal();
            constructors[DistributedObjectInfo] = arg => new DistributedObjectInfo();
            //constructors[GetDistributedObjectInfo] = arg => new GetDistributedObjectsRequest();
            //constructors[CreateProxy] = arg => new ClientCreateRequest();
            //constructors[DestroyProxy] = arg => new ClientDestroyRequest();
            //constructors[Listener] = arg => new DistributedObjectListenerRequest();

            //constructors[MembershipListener] = arg => new AddMembershipListenerRequest();
            //constructors[ClientPing] = arg => new ClientPingRequest();
            //constructors[GetPartitions] = arg => new GetPartitionsRequest();
            //constructors[RemoveListener] = arg => new RemoveDistributedObjectListenerRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            var builder = new ClassDefinitionBuilder(Id, Principal);
            builder.AddUTFField("uuid").AddUTFField("ownerUuid");
            return new List<IClassDefinition>(1) {builder.Build()};
        }
    }
}