using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Atomiclong;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class AtomicLongPortableHook : IPortableHook
    {
        public const int AddAndGet = 1;
        public const int CompareAndSet = 2;
        public const int GetAndAdd = 3;
        public const int GetAndSet = 4;
        public const int Set = 5;
        public const int Apply = 6;
        public const int Alter = 7;
        public const int AlterAndGet = 8;
        public const int GetAndAlter = 9;
        public const int Size = GetAndAlter +1;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.AtomicLongPortableFactory, -17);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            return new ArrayPortableFactory();
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}