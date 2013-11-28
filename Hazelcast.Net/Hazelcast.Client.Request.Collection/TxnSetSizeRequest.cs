using Hazelcast.Client.Request.Collection;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class TxnSetSizeRequest : TxnCollectionRequest
	{
		public TxnSetSizeRequest()
		{
		}

		public TxnSetSizeRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.TxnSetSize;
		}
	}
}
