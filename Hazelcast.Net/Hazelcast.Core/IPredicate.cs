using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    public interface IPredicate<K, V> : IDataSerializable
    {
    }
}