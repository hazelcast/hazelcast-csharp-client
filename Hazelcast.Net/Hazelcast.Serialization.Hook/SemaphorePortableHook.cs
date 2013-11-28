using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Serialization.Hook
{
	
	public class SemaphorePortableHook : IPortableHook
	{
		public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.SemaphorePortableFactory, -16);
		public const int Acquire = 1;
		public const int Available = 2;
		public const int Drain = 3;
		public const int Init = 4;
		public const int Reduce = 5;
		public const int Release = 6;

		public virtual int GetFactoryId()
		{
			return FId;
		}

		public virtual IPortableFactory CreateFactory()
        {

            var constructors = new Func<int, IPortable>[Release + 1];
            constructors[Acquire] = arg => new AcquireRequest();
            constructors[Available] = arg => new AvailableRequest();
            constructors[Drain] = arg => new DrainRequest();
            constructors[Init] = arg => new InitRequest();
            constructors[Reduce] = arg => new ReduceRequest();
            constructors[Release] = arg => new ReleaseRequest();
            return new ArrayPortableFactory(constructors);

        }

		public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
		{
			return null;
		}
	}
}
