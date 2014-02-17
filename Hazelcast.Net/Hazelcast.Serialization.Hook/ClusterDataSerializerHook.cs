using System;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal sealed class ClusterDataSerializerHook : DataSerializerHook
    {
        public const int FId = 0;
        public const int Data = 0;
        public const int Address = 1;
        public const int Member = 2;
        public const int Heartbeat = 3;
        public const int ConfigCheck = 4;

        public const int MembershipEvent = 8;

        public int GetFactoryId()
        {
            return FId;
        }

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[MembershipEvent + 1];
            constructors[Data] = arg => new Data();
            constructors[Address] = arg => new Address();
            constructors[Member] = arg => new Member();
            //constructors[Heartbeat] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //constructors[ConfigCheck] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[MembershipEvent] = arg => new ClientMembershipEvent();
            return new ArrayDataSerializableFactory(constructors);
        }
    }
}