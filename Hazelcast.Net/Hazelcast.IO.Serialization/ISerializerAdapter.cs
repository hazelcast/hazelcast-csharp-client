namespace Hazelcast.IO.Serialization
{
    internal interface ISerializerAdapter
    {
        /// <exception cref="System.IO.IOException"></exception>
        void Write(IObjectDataOutput output, object obj);

        /// <exception cref="System.IO.IOException"></exception>
        object Read(IObjectDataInput input);

        /// <exception cref="System.IO.IOException"></exception>
        byte[] Write(object obj);

        /// <exception cref="System.IO.IOException"></exception>
        object Read(Data data);

        int GetTypeId();

        void Destroy();

        ISerializer GetImpl();
    }
}