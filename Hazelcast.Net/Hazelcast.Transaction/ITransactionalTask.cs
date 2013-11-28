using Hazelcast.Transaction;


namespace Hazelcast.Transaction
{
	/// <summary>Contains the logic that is going to be executed within a transaction.</summary>
	/// <remarks>
	/// Contains the logic that is going to be executed within a transaction. In practice the
	/// implementation will be an anonymous inner task.
	/// Unlike the
	/// <see cref="Hazelcast.Net.Ext.Runnable">Hazelcast.Net.Ext.Runnable</see>
	/// and
	/// <see cref="Hazelcast.Net.Ext.Callable{V}">Hazelcast.Net.Ext.Callable&lt;V&gt;</see>
	/// the
	/// <see cref="IITransactionalTask{T}">ITransactionalTask&lt;T&gt;</see>
	/// will run on the caller thread.
	/// </remarks>
	
	/// <seealso cref="Hazelcast.Core.HazelcastInstance.ExecuteTransaction{T}(ITransactionalTask{T})">Hazelcast.Core.IHazelcastInstance.ExecuteTransaction&lt;T&gt;(ITransactionalTask&lt;T&gt;)</seealso>
	/// <seealso cref="Hazelcast.Core.HazelcastInstance.ExecuteTransaction{T}(TransactionOptions, ITransactionalTask{T})">Hazelcast.Core.IHazelcastInstance.ExecuteTransaction&lt;T&gt;(TransactionOptions, ITransactionalTask&lt;T&gt;)</seealso>
	public interface ITransactionalTask<T>
	{
		/// <summary>Executes the transactional logic.</summary>
		/// <remarks>Executes the transactional logic.</remarks>
		/// <param name="context">
		/// the ITransactionalTaskContext that provides access to the transaction and to the
		/// transactional resourcs like the
		/// <see cref="Hazelcast.Core.ITransactionalMap{K, V}">Hazelcast.Core.ITransactionalMap&lt;K, V&gt;</see>
		/// .
		/// </param>
		/// <returns>the result of the task</returns>
		/// <exception cref="TransactionException">if transaction error happens while executing this task.</exception>
		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		T Execute(ITransactionalTaskContext context);
	}
}
