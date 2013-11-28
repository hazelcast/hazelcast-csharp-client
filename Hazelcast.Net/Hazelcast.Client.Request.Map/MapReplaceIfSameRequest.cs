using Hazelcast.Client.Request.Map;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapReplaceIfSameRequest : MapPutRequest
	{
		private Data testValue;

		public MapReplaceIfSameRequest()
		{
		}

		public MapReplaceIfSameRequest(string name, Data key, Data testValue, Data value, int threadId) : base(name, key, value, threadId)
		{
			this.testValue = testValue;
		}

		public override int GetClassId()
		{
			return MapPortableHook.ReplaceIfSame;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			IObjectDataOutput output = writer.GetRawDataOutput();
			testValue.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			IObjectDataInput input = reader.GetRawDataInput();
			testValue = new Data();
			testValue.ReadData(input);
		}
	}
}
