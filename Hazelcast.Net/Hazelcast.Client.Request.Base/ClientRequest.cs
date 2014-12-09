using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Base
{
	public abstract class ClientRequest : IVersionedPortable
	{
		protected internal int callId = -1;

		public virtual int GetCallId()
		{
			return callId;
		}

		public virtual void SetCallId(int callId)
		{
			this.callId = callId;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WritePortable(IPortableWriter writer)
		{
			writer.WriteInt("cId", callId);
			Write(writer);
		}

	    public void ReadPortable(IPortableReader reader)
	    {
	        throw new System.NotImplementedException();
	    }

		/// <exception cref="System.IO.IOException"></exception>
		public abstract void Write(IPortableWriter writer);

		public virtual int GetClassVersion()
		{
			return 1;
		}

		public abstract int GetClassId();

		public abstract int GetFactoryId();
	}
}
