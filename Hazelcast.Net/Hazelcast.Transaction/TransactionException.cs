using System;
using Hazelcast.Core;


namespace Hazelcast.Transaction
{
	/// <summary>
	/// A
	/// <see cref="Hazelcast.Core.HazelcastException">Hazelcast.Core.HazelcastException</see>
	/// that is thrown when something goes wrong while dealing with transactions and transactional
	/// data-structures.
	/// </summary>
	[System.Serializable]
	public class TransactionException : HazelcastException
	{
		public TransactionException()
		{
		}

		public TransactionException(string message) : base(message)
		{
		}

		public TransactionException(string message, Exception cause) : base(message, cause)
		{
		}

		public TransactionException(Exception cause) : base(cause)
		{
		}
	}
}
