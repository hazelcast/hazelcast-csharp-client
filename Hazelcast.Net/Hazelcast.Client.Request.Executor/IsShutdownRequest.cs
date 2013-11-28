using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Executor
{
	
	[System.Serializable]
	public class IsShutdownRequest : IIdentifiedDataSerializable, IRetryableRequest
	{
		internal string name;

		public IsShutdownRequest()
		{
		}

		public IsShutdownRequest(string name)
		{
			this.name = name;
		}

		public virtual int GetFactoryId()
		{
			return ExecutorDataSerializerHook.FId;
		}

		public virtual int GetId()
		{
			return ExecutorDataSerializerHook.IsShutdownRequest;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WriteData(IObjectDataOutput output)
		{
			output.WriteUTF(name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadData(IObjectDataInput input)
		{
			name = input.ReadUTF();
		}
	}
}
