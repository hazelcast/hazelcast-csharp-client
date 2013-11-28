using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class LockPortableHook : IPortableHook
    {
        public const int Lock = 1;
        public const int Unlock = 2;
        public const int IsLocked = 3;
        public const int GetLockCount = 5;
        public const int GetRemainingLease = 6;
        public static readonly int FactoryId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.LockPortableFactory, -15);

        public virtual int GetFactoryId()
        {
            return FactoryId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[GetRemainingLease + 1];
            constructors[Lock] = arg => new LockRequest();
            constructors[Unlock] = arg => new UnlockRequest();
            constructors[IsLocked] = arg => new IsLockedRequest();
            constructors[GetLockCount] = arg => new GetLockCountRequest();
            constructors[GetRemainingLease] = arg => new GetRemainingLeaseRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}