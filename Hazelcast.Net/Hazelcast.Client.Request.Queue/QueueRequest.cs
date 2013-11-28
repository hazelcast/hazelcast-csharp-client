using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Queue
{
	
	public abstract class QueueRequest : IPortable
	{
		protected internal string name;

		protected internal long timeoutMillis;

		protected internal QueueRequest()
		{
		}

		protected internal QueueRequest(string name)
		{
			this.name = name;
		}

		protected internal QueueRequest(string name, long timeoutMillis)
		{
			this.name = name;
			this.timeoutMillis = timeoutMillis;
		}

		public virtual int GetFactoryId()
		{
			return QueuePortableHook.FId;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteLong("t", timeoutMillis);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			timeoutMillis = reader.ReadLong("t");
		}

		public abstract int GetClassId();
	}
}
