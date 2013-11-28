using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public abstract class CollectionRequest : IPortable
	{
		protected internal string serviceName;

		protected internal string name;

		public CollectionRequest()
		{
		}

		public CollectionRequest(string name)
		{
			this.name = name;
		}

		public virtual void SetServiceName(string serviceName)
		{
			this.serviceName = serviceName;
		}

		public virtual int GetFactoryId()
		{
			return CollectionPortableHook.FId;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("s", serviceName);
			writer.WriteUTF("n", name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			serviceName = reader.ReadUTF("s");
			name = reader.ReadUTF("n");
		}

		public abstract int GetClassId();
	}
}
