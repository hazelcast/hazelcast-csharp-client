namespace Hazelcast.IO.Serialization
{
    public interface IByteArraySerializer<T> : ISerializer
    {
        /// <exception cref="System.IO.IOException"></exception>
        byte[] Write(T _object);

        /// <exception cref="System.IO.IOException"></exception>
        T Read(byte[] buffer);
    }
}