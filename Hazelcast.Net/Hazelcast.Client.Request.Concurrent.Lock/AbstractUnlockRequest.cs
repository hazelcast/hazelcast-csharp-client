using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Client.Request.Concurrent.Lock
{
	
	public abstract class AbstractUnlockRequest : IPortable
	{
		protected internal Data key;

		private int threadId;

		private bool force;

		public AbstractUnlockRequest()
		{
		}

		public AbstractUnlockRequest(Data key, int threadId)
		{
			this.key = key;
			this.threadId = threadId;
		}

		protected internal AbstractUnlockRequest(Data key, int threadId, bool force)
		{
			this.key = key;
			this.threadId = threadId;
			this.force = force;
		}

		protected internal object GetKey()
		{
			return key;
		}

		public string GetServiceName()
		{
			return ServiceNames.Lock;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteInt("tid", threadId);
			writer.WriteBoolean("force", force);
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			threadId = reader.ReadInt("tid");
			force = reader.ReadBoolean("force");
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
		}

		public abstract int GetClassId();

		public abstract int GetFactoryId();
	}
}
