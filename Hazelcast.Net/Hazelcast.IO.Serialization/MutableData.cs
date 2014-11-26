namespace Hazelcast.IO.Serialization
{
	internal interface IMutableData : IData
	{
		byte[] GetData();

		void SetData(byte[] data);

		void SetType(int type);

		void SetPartitionHash(int partitionHash);

		void SetHeader(byte[] header);
	}
}
