using Hazelcast.Core;
using Hazelcast.Transaction;


namespace Hazelcast.Transaction
{
	/// <summary>
	/// Provides a context to do transactional operations; so beginning/committing transactions, but also retrieving
	/// transactional data-structures like the
	/// <see cref="ITransactionalMap{K,V}">Hazelcast.Core.ITransactionalMap&lt;K, V&gt;</see>
	/// .
	/// </summary>
	public interface ITransactionContext : ITransactionalTaskContext
	{
		/// <summary>Begins a transaction.</summary>
		/// <remarks>Begins a transaction.</remarks>
		/// <exception cref="System.InvalidOperationException">if a transaction already is active.</exception>
		void BeginTransaction();

		/// <summary>Commits a transaction.</summary>
		/// <remarks>Commits a transaction.</remarks>
		/// <exception cref="TransactionException">if no transaction is active or the transaction could not be committed.</exception>
		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		void CommitTransaction();

		/// <summary>Rollback of the current transaction.</summary>
		/// <remarks>Rollback of the current transaction.</remarks>
		/// <exception cref="System.InvalidOperationException">if no there is no active transaction.</exception>
		void RollbackTransaction();

		/// <summary>Gets the id that uniquely identifies the transaction.</summary>
		/// <remarks>Gets the id that uniquely identifies the transaction.</remarks>
		/// <returns>the transaction id.</returns>
		string GetTxnId();
	}
}
