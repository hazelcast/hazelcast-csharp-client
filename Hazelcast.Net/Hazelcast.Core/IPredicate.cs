using System.Collections.Generic;
using Hazelcast.IO;

namespace Hazelcast.Core
{
    /// <summary>IPredicate instance must be thread-safe.</summary>
    /// <remarks>
    ///     IPredicate instance must be thread-safe.
    ///     <see cref="IPredicate{K,V}.Apply(System.Collections.DictionaryEntry{K, V})">
    ///         IPredicate&lt;K, V&gt;
    ///         .Apply(System.Collections.DictionaryEntry&lt;K, V&gt;)
    ///     </see>
    ///     is called by multiple threads concurrently.
    /// </remarks>
    /// <?></?>
    /// <?></?>
    public interface IPredicate<K, V> : IObjectDataOutput
    {
        bool Apply(KeyValuePair<K, V> mapEntry);
    }
}