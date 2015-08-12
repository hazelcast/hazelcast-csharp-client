using System;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class ClientTxnPortableHook : IPortableHook
    {
        public const int Create = 1;
        public const int Commit = 2;
        public const int Rollback = 3;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ClientTxnPortableFactory, -19);

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