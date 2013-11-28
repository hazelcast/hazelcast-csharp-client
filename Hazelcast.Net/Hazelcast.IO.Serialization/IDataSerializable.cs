namespace Hazelcast.IO.Serialization
{
    public interface IDataSerializable
    {
        /// <exception cref="System.IO.IOException"></exception>
        void WriteData(IObjectDataOutput output);

        /// <exception cref="System.IO.IOException"></exception>
        void ReadData(IObjectDataInput input);
    }
}