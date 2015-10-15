using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
	internal interface ISerializationServiceBuilder
	{
		ISerializationServiceBuilder SetVersion(byte version);
	    ISerializationServiceBuilder SetPortableVersion(int version);
		ISerializationServiceBuilder SetConfig(SerializationConfig config);
		ISerializationServiceBuilder AddDataSerializableFactory(int id, IDataSerializableFactory factory);
		ISerializationServiceBuilder AddPortableFactory(int id, IPortableFactory factory);
		ISerializationServiceBuilder AddClassDefinition(IClassDefinition cd);
		ISerializationServiceBuilder SetCheckClassDefErrors(bool checkClassDefErrors);
		ISerializationServiceBuilder SetManagedContext(IManagedContext managedContext);
		ISerializationServiceBuilder SetUseNativeByteOrder(bool useNativeByteOrder);
		ISerializationServiceBuilder SetByteOrder(ByteOrder byteOrder);
		ISerializationServiceBuilder SetHazelcastInstance(IHazelcastInstance hazelcastInstance);
		ISerializationServiceBuilder SetEnableCompression(bool enableCompression);
		ISerializationServiceBuilder SetEnableSharedObject(bool enableSharedObject);
		ISerializationServiceBuilder SetPartitioningStrategy(IPartitioningStrategy partitionStrategy);
		ISerializationServiceBuilder SetInitialOutputBufferSize(int initialOutputBufferSize);
		ISerializationService Build();
	}
}
