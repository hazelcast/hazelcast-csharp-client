using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Topic
{
	
	public class AddMessageListenerRequest : IPortable
	{
		private string name;

		public AddMessageListenerRequest()
		{
		}

		public AddMessageListenerRequest(string name)
		{
			this.name = name;
		}

		public virtual int GetFactoryId()
		{
			return TopicPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return TopicPortableHook.AddListener;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
		}
	}
}
