using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Transaction
{
    [Serializable]
    internal class TransactionTimedOutException : TransactionException
    {
        public TransactionTimedOutException()
        {
        }

        public TransactionTimedOutException(string message)
            : base(message)
        {
        }
    }
}
