using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
	
	public sealed class CountDownRequest : IPortable
	{
		private string name;

		public CountDownRequest()
		{
		}

		public CountDownRequest(string name)
		{
			this.name = name;
		}

		public int GetFactoryId()
		{
			return CountDownLatchPortableHook.FId;
		}

		public int GetClassId()
		{
			return CountDownLatchPortableHook.CountDown;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("name", name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("name");
		}
	}
}
