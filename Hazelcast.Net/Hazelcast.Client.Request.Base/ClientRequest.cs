using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Base
{
	public abstract class ClientRequest : IVersionedPortable
	{
		protected internal int callId = -1;

        [NonSerialized] 
        private bool singleConnection;

	    public int CallId
	    {
	        get { return callId; }
	        set { callId = value; }
	    }

	    public bool SingleConnection
	    {
	        get { return singleConnection; }
	        set { singleConnection = value; }
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
