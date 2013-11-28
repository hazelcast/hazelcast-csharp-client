using Hazelcast.Client.Request.Cluster;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Cluster
{
	
	[System.Serializable]
	public sealed class ClientPingRequest : IIdentifiedDataSerializable
	{
		public int GetFactoryId()
		{
			return ClusterDataSerializerHook.FId;
		}

		public int GetId()
		{
			return ClusterDataSerializerHook.Ping;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WriteData(IObjectDataOutput output)
		{
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadData(IObjectDataInput input)
		{
		}
	}
}
