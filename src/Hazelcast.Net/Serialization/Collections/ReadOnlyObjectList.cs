using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Serialization.Collections
{
    public class ReadOnlyObjectList<T>: IReadOnlyList<object>
    {
        private readonly IList<T> _list;

        public int Count => _list.Count;

        public ReadOnlyObjectList(IList<T> list)
        {
            _list = list;
        }

        // If T is struct, this leads to boxing on each enumeration
        public IEnumerator<object> GetEnumerator() => _list.Cast<object>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // If T is struct, this leads to boxing on each access
        public object this[int index] => _list[index];
    }
}
