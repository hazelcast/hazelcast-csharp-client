using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>Concurrent, blocking, distributed, observable queue.</summary>
    /// <remarks>Concurrent, blocking, distributed, observable queue.</remarks>
    /// <seealso cref="IBaseQueue{E}">BaseQueue&lt;E&gt;</seealso>
    public interface IQueue<E> : IBaseQueue<E>, IHazelcastCollection<E>
    {
        //int Count { get; }
        //new bool Add(E e);
        //bool Offer(E e);
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        void Put(E e);

        //bool Offer(E e, long timeout, TimeUnit unit);
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        E Take();

        //E Poll(long timeout, TimeUnit unit);
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int RemainingCapacity();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool Remove(object o);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool Contains(object o);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int DrainTo<T>(ICollection<T> c) where T : E;

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int DrainTo<T>(ICollection<T> c, int maxElements) where T : E;

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        E Remove();

        //E Poll();
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        E Element();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        E Peek();

/*
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool IsEmpty();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        IEnumerator<E> GetEnumerator();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        object[] ToArray();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        T[] ToArray<T>(T[] a);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool ContainsAll<_T0>(ICollection<_T0> c);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool AddAll<_T0>(ICollection<_T0> c) where _T0 : E;

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool RemoveAll<_T0>(ICollection<_T0> c);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool RetainAll<_T0>(ICollection<_T0> c);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        void Clear();
*/
        //    /**
        //     * Returns LocalQueueStats for this queue.
        //     * LocalQueueStats is the statistics for the local portion of this
        //     * queue.
        //     *
        //     * @return this queue's local statistics.
        //     */
        //    LocalQueueStats getLocalQueueStats();
    }
}