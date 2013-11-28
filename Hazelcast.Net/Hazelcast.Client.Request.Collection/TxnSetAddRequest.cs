using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class TxnSetAddRequest : TxnCollectionRequest
	{
		public TxnSetAddRequest()
		{
		}

		public TxnSetAddRequest(string name, Data value) : base(name, value)
		{
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.TxnSetAdd;
		}
	}
}
