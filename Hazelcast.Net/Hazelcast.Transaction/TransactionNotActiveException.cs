using System;
using Hazelcast.Core;

namespace Hazelcast.Transaction
{
    /// <summary>
    ///     A
    ///     <see cref="Hazelcast.Core.HazelcastException">Hazelcast.Core.HazelcastException</see>
    ///     thrown when an a transactional operation is executed without an active transaction.
    /// </summary>
    [Serializable]
    internal class TransactionNotActiveException : HazelcastException
    {
        public TransactionNotActiveException()
        {
        }

        public TransactionNotActiveException(string message) : base(message)
        {
        }
    }
}