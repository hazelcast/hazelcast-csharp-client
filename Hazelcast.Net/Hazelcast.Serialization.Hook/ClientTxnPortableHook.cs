using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class ClientTxnPortableHook : IPortableHook
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
            var constructors = new Func<int, IPortable>[Rollback + 1];
            constructors[Create] = arg => new CreateTransactionRequest();
            constructors[Commit] = arg => new CommitTransactionRequest();
            constructors[Rollback] = arg => new RollbackTransactionRequest();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}