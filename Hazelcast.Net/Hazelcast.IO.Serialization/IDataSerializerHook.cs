namespace Hazelcast.IO.Serialization
{
	public abstract class IDataSerializerHook
	{
		public const int F_ID_OFFSET_WEBMODULE = -1000;

		public abstract int GetFactoryId();

		public abstract IDataSerializableFactory CreateFactory();
	}
}
