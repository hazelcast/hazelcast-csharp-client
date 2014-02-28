using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Concurrent, distributed implementation of <see cref="IList{T}"/>IList 
    /// </summary>
    public interface IHList<E> : IList<E>, IHCollection<E>
    {
        E Get(int index);
        E Set(int index, E element);

        void Add(int index, E element);

        E Remove(int index);

        int LastIndexOf(E o);

        bool AddAll<_T0>(int index, ICollection<_T0> c) where _T0 : E;

        IList<E> SubList(int fromIndex, int toIndex);

        //int IndexOf(E item);
        //void Insert(int index, E item);
        //void RemoveAt(int index);
        //E this[int index] { get; set; }
    }
}