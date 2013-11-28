using Hazelcast.Core;
using Hazelcast.Transaction;


namespace Hazelcast.Transaction
{
	/// <summary>Marker interface for all transactional distributed objects.</summary>
	/// <remarks>Marker interface for all transactional distributed objects.</remarks>
	public interface ITransactionalObject : IDistributedObject
	{
	}
}
