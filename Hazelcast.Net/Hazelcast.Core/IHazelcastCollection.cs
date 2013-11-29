using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>Concurrent, distributed, partitioned, listenable collection.</summary>
    /// <remarks>Concurrent, distributed, partitioned, listenable collection.</remarks>
    public interface IHazelcastCollection<T> : ICollection<T>, JCollection<T>, IDistributedObject
    {
        //string GetName();
        /// <summary>Returns the name of this collection</summary>
        /// <returns>name of this collection</returns>
        /// <summary>Adds an item listener for this collection.</summary>
        /// <remarks>
        ///     Adds an item listener for this collection. Listener will get notified
        ///     for all collection add/remove events.
        /// </remarks>
        /// <param name="listener">item listener</param>
        /// <param name="includeValue">
        ///     <tt>true</tt> updated item should be passed
        ///     to the item listener, <tt>false</tt> otherwise.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddItemListener(IItemListener<T> listener, bool includeValue);

        /// <summary>Removes the specified item listener.</summary>
        /// <remarks>
        ///     Removes the specified item listener.
        ///     Returns silently if the specified listener is not added before.
        /// </remarks>
        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveItemListener(string registrationId);

        //override ICollection add to return bool
        new bool Add(T item);
    }

    public interface JCollection<E> : IEnumerable<E>
    {
        //int size();
        int Size();

        //boolean isEmpty();
        bool IsEmpty();


        //bool contains(Object o);
        //bool Contains(T item);


        //Iterator<E> iterator();
        //IEnumerator<T> GetEnumerator();

        //Object[] toArray();
        E[] ToArray();

        //<T> T[] toArray(T[] a);
        T[] ToArray<T>(T[] a);

        //boolean add(E e);
        //bool Add(E item);


        //boolean remove(Object o);
        //bool Remove(T item);


        //boolean containsAll(Collection<?> c);
        bool ContainsAll<T>(ICollection<T> c);


        //boolean removeAll(Collection<?> c);
        bool RemoveAll<T>(ICollection<T> c);


        //boolean retainAll(Collection<?> c);
        bool RetainAll<T>(ICollection<T> c);


        //boolean addAll(Collection<? extends E> c);
        bool AddAll<T>(ICollection<T> c);


        //void clear();
        //void Clear();
    }
}