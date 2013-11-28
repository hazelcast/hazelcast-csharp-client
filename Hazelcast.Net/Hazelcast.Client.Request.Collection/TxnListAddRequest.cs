using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class TxnListAddRequest : TxnCollectionRequest
	{
		public TxnListAddRequest()
		{
		}

		public TxnListAddRequest(string name, Data value) : base(name, value)
		{
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.TxnListAdd;
		}
	}
}
