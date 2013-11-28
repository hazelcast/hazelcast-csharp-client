using Hazelcast.Net.Ext;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Transactional implementation of
    ///     <see cref="IBaseQueue{E}">BaseQueue&lt;E&gt;</see>
    ///     .
    /// </summary>
    /// <seealso cref="IBaseQueue{E}">BaseQueue&lt;E&gt;</seealso>
    /// <seealso cref="IQueue{E}">IQueue&lt;E&gt;</seealso>
    /// <?></?>
    public interface ITransactionalQueue<E> : ITransactionalObject, IBaseQueue<E>
    {
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool Offer(E e);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        bool Offer(E e, long timeout, TimeUnit unit);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        E Poll();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        E Poll(long timeout, TimeUnit unit);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        E Peek();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        E Peek(long timeout, TimeUnit unit);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int Size();
    }
}