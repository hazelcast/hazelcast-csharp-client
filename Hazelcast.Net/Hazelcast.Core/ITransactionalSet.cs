using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Transactional implementation of
    ///     <see cref="IHSet{E}">IHSet&lt;E&gt;</see>
    ///     .
    /// </summary>
    public interface ITransactionalSet<E> : ITransactionalObject
    {
        /// <summary>Add new item to transactional set</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is added successfully</returns>
        bool Add(E e);

        /// <summary>Add item from transactional set</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is remove successfully</returns>
        bool Remove(E e);

        /// <summary>Returns the size of the set</summary>
        /// <returns>size</returns>
        int Size();
    }
}