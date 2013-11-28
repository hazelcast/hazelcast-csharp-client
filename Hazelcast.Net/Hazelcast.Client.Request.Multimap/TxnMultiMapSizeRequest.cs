using Hazelcast.Client.Request.Multimap;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class TxnMultiMapSizeRequest : TxnMultiMapRequest
	{
		public TxnMultiMapSizeRequest()
		{
		}

		public TxnMultiMapSizeRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.TxnMmSize;
		}
	}
}
