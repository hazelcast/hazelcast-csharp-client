using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Transactional implementation of
    ///     <see cref="IHazelcastList{E}">IHazelcastList&lt;E&gt;</see>
    ///     .
    /// </summary>
    public interface ITransactionalList<E> : ITransactionalObject
    {
        /// <summary>Add new item to transactional list</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is added successfully</returns>
        bool Add(E e);

        /// <summary>Add item from transactional list</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is remove successfully</returns>
        bool Remove(E e);

        /// <summary>Returns the size of the list</summary>
        /// <returns>size</returns>
        int Size();
    }
}