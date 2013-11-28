using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Client.Request.Concurrent.Lock
{
	
	public abstract class AbstractIsLockedRequest : IPortable
	{
		protected internal Data key;

		private int threadId;

		public AbstractIsLockedRequest()
		{
		}

		public AbstractIsLockedRequest(Data key)
		{
			this.key = key;
			this.threadId = -1;
		}

		protected internal AbstractIsLockedRequest(Data key, int threadId)
		{
			this.key = key;
			this.threadId = threadId;
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
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			threadId = reader.ReadInt("tid");
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
		}

		public abstract int GetClassId();

		public abstract int GetFactoryId();
	}
}
