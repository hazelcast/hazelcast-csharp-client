using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Countdownlatch;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class CountDownLatchPortableHook : IPortableHook
    {
        public const int CountDown = 1;
        public const int Await = 2;
        public const int SetCount = 3;
        public const int GetCount = 4;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.CdlPortableFactory, -14);

        public int GetFactoryId()
        {
            return FId;
        }

        public IPortableFactory CreateFactory()
        {
            return new ArrayPortableFactory();
        }

        public ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}