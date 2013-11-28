using Hazelcast.Client.Request.Collection;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class TxnListSizeRequest : TxnCollectionRequest
	{
		public TxnListSizeRequest()
		{
		}

		public TxnListSizeRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.TxnListSize;
		}
	}
}
