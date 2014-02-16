using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class SemaphorePortableHook : IPortableHook
    {
        public const int Acquire = 1;
        public const int Available = 2;
        public const int Drain = 3;
        public const int Init = 4;
        public const int Reduce = 5;
        public const int Release = 6;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.SemaphorePortableFactory, -16);

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