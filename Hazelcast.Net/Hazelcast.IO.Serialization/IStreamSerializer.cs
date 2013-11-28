namespace Hazelcast.IO.Serialization
{
    public interface IStreamSerializer<T> : ISerializer
    {
        /// <exception cref="System.IO.IOException"></exception>
        void Write(IObjectDataOutput output, T t);

        /// <exception cref="System.IO.IOException"></exception>
        T Read(IObjectDataInput input);
    }
}