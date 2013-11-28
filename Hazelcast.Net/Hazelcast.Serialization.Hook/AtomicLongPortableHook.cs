using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Atomiclong;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class AtomicLongPortableHook : IPortableHook
    {
        public const int AddAndGet = 1;
        public const int CompareAndSet = 2;
        public const int GetAndAdd = 3;
        public const int GetAndSet = 4;
        public const int Set = 5;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.AtomicLongPortableFactory, -17);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[Set + 1];
            constructors[AddAndGet] = arg => new AddAndGetRequest();
            constructors[CompareAndSet] = arg => new CompareAndSetRequest();
            constructors[GetAndAdd] = arg => new GetAndAddRequest();
            constructors[GetAndSet] = arg => new GetAndSetRequest();
            constructors[Set] = arg => new SetRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}