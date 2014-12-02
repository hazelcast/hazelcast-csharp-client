using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class LockPortableHook : IPortableHook
    {
        public const int Lock = 1;
        public const int Unlock = 2;
        public const int IsLocked = 3;
        public const int GetLockCount = 5;
        public const int GetRemainingLease = 6;
        public const int ConditionBeforeAwait = 7;
        public const int ConditionAwait = 8;
        public const int ConditionSignal = 9;

        public static readonly int FactoryId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.LockPortableFactory, -15);

        public virtual int GetFactoryId()
        {
            return FactoryId;
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