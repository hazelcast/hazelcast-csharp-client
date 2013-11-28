using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class TxnSetRemoveRequest : TxnCollectionRequest
	{
		public TxnSetRemoveRequest()
		{
		}

		public TxnSetRemoveRequest(string name, Data value) : base(name, value)
		{
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.TxnSetRemove;
		}
	}
}
