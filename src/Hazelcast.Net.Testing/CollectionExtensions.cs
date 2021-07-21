using System.Collections.Generic;

namespace Hazelcast.Testing
{
    public static class CollectionExtensions
    {
        public static T GetOrDefault<T>(this IList<T> source, int index) => (source.Count > index || index < 0)
            ? source[index]
            : default;
    }
}
