using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Transaction
{
	
	public class CommitTransactionRequest : IPortable
	{
		public CommitTransactionRequest()
		{
		}

		public virtual int GetFactoryId()
		{
			return ClientTxnPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return ClientTxnPortableHook.Commit;
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
