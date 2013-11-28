using System;
using System.Collections.Generic;
using Hazelcast.Client;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Serialization.Hook
{
	
	public class ClientPortableHook : IPortableHook
	{
		public static readonly int Id = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ClientPortableFactory, -3);
		public const int GenericError = 1;
		public const int Auth = 2;
		public const int Principal = 3;
		public const int GetDistributedObjectInfo = 4;
		public const int DistributedObjectInfo = 6;
		public const int CreateProxy = 7;
		public const int DestroyProxy = 8;
		public const int Listener = 9;

		public virtual int GetFactoryId()
		{
			return Id;
		}

		public virtual IPortableFactory CreateFactory()
		{
            var constructors = new Func<int, IPortable>[Listener + 1];
            constructors[GenericError] = arg => new GenericError();
            constructors[Auth] = arg => new AuthenticationRequest();
            constructors[Principal] = arg => new ClientPrincipal();
            constructors[GetDistributedObjectInfo] = arg => new GetDistributedObjectsRequest();
            constructors[DistributedObjectInfo] = arg => new DistributedObjectInfo();
            constructors[CreateProxy] = arg => new ClientCreateRequest();
            constructors[DestroyProxy] = arg => new ClientDestroyRequest();
            constructors[Listener] = arg => new DistributedObjectListenerRequest();
            return new ArrayPortableFactory(constructors);
		}

		public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
		{
			var builder = new ClassDefinitionBuilder(Id, Principal);
			builder.AddUTFField("uuid").AddUTFField("ownerUuid");
            return new List<IClassDefinition>(1) { builder.Build() };
		}
	}
}
