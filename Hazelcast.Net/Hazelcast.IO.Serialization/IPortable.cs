namespace Hazelcast.IO.Serialization
{
    public interface IPortable
    {
        int GetFactoryId();

        int GetClassId();

        /// <exception cref="System.IO.IOException"></exception>
        void WritePortable(IPortableWriter writer);

        /// <exception cref="System.IO.IOException"></exception>
        void ReadPortable(IPortableReader reader);
    }
}