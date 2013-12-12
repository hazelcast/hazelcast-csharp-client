using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    /// <summary>IPredicate instance must be thread-safe.</summary>
    /// <remarks>IPredicate instance must be thread-safe. </remarks>
    public interface IPredicate<K, V> : IDataSerializable
    {
    }
}