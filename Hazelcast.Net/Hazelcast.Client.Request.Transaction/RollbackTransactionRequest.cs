using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Transaction
{
	
	public class RollbackTransactionRequest : IPortable
	{
		public RollbackTransactionRequest()
		{
		}

		public virtual int GetFactoryId()
		{
			return ClientTxnPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return ClientTxnPortableHook.Rollback;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
		}
	}
}
