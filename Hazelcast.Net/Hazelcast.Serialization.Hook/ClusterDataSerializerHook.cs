using System;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Serialization.Hook
{
	
	public sealed class ClusterDataSerializerHook : DataSerializerHook
	{
        public const int FId = 0;
		public const int Data = 0;
		public const int Address = 1;
		public const int Member = 2;
		public const int Heartbeat = 3;
		public const int ConfigCheck = 4;
		public const int AddMsListener = 7;
		public const int MembershipEvent = 8;
		public const int Ping = 9;

		public int GetFactoryId()
		{
			return FId;
		}

		public IDataSerializableFactory CreateFactory()
		{
            var constructors = new Func<int, IIdentifiedDataSerializable>[Ping + 1];
            constructors[Data] = arg => new Data();
            constructors[Address] = arg => new Address();
            constructors[Member] = arg => new Member();
		    constructors[Heartbeat] = delegate(int arg) { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[ConfigCheck] = delegate(int arg) { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[AddMsListener] = arg => new AddMembershipListenerRequest();
            constructors[MembershipEvent] = arg => new ClientMembershipEvent();
            constructors[Ping] = arg => new ClientPingRequest();
            return new ArrayDataSerializableFactory(constructors);
		}
	}
}
