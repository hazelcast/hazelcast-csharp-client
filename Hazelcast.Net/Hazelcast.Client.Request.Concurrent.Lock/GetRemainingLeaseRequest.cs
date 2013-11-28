using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Lock
{
	
	public sealed class GetRemainingLeaseRequest : IPortable
	{
		private Data key;

		public GetRemainingLeaseRequest()
		{
		}

		public GetRemainingLeaseRequest(Data key)
		{
			this.key = key;
		}


		public int GetFactoryId()
		{
			return LockPortableHook.FactoryId;
		}

		public int GetClassId()
		{
			return LockPortableHook.GetRemainingLease;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WritePortable(IPortableWriter writer)
		{
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadPortable(IPortableReader reader)
		{
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
		}
	}
}
