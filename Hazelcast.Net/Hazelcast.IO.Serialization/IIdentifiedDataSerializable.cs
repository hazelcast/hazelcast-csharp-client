using System;

namespace Hazelcast.IO.Serialization
{
    public interface IIdentifiedDataSerializable : IDataSerializable
    {
        int GetFactoryId();

        int GetId();
    }

    public class IdentifiedDataSerializable
    {
        public string GetJavaClassName() { throw new NotSupportedException();}
    }
}