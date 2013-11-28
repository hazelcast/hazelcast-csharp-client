using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>Base interface for Hazelcast distributed queues.</summary>
    /// <remarks>Base interface for Hazelcast distributed queues.</remarks>
    /// <seealso cref="IQueue{E}">IQueue&lt;E&gt;</seealso>
    /// <seealso cref="ITransactionalQueue{E}">ITransactionalQueue&lt;E&gt;</seealso>
    /// <?></?>
    public interface IBaseQueue<E> : IDistributedObject
    {
        /// <summary>
        ///     Inserts the specified element into this queue if it is possible to do
        ///     so immediately without violating capacity restrictions, returning
        ///     <tt>true</tt> upon success and <tt>false</tt> if no space is currently
        ///     available.
        /// </summary>
        /// <remarks>
        ///     Inserts the specified element into this queue if it is possible to do
        ///     so immediately without violating capacity restrictions, returning
        ///     <tt>true</tt> upon success and <tt>false</tt> if no space is currently
        ///     available.
        /// </remarks>
        /// <param name="e">the element to add</param>
        /// <returns>
        ///     <tt>true</tt> if the element was added to this queue, else
        ///     <tt>false</tt>
        /// </returns>
        bool Offer(E e);

        /// <summary>
        ///     Inserts the specified element into this queue, waiting up to the
        ///     specified wait time if necessary for space to become available.
        /// </summary>
        /// <remarks>
        ///     Inserts the specified element into this queue, waiting up to the
        ///     specified wait time if necessary for space to become available.
        /// </remarks>
        /// <param name="e">the element to add</param>
        /// <param name="timeout">
        ///     how long to wait before giving up, in units of
        ///     <tt>unit</tt>
        /// </param>
        /// <param name="unit">
        ///     a <tt>TimeUnit</tt> determining how to interpret the
        ///     <tt>timeout</tt> parameter
        /// </param>
        /// <returns>
        ///     <tt>true</tt> if successful, or <tt>false</tt> if
        ///     the specified waiting time elapses before space is available
        /// </returns>
        /// <exception cref="System.Exception">if interrupted while waiting</exception>
        bool Offer(E e, long timeout, TimeUnit unit);

        /// <summary>
        ///     Retrieves and removes the head of this queue,
        ///     or returns <tt>null</tt> if this queue is empty.
        /// </summary>
        /// <remarks>
        ///     Retrieves and removes the head of this queue,
        ///     or returns <tt>null</tt> if this queue is empty.
        /// </remarks>
        /// <returns>the head of this queue, or <tt>null</tt> if this queue is empty</returns>
        E Poll();

        /// <summary>
        ///     Retrieves and removes the head of this queue, waiting up to the
        ///     specified wait time if necessary for an element to become available.
        /// </summary>
        /// <remarks>
        ///     Retrieves and removes the head of this queue, waiting up to the
        ///     specified wait time if necessary for an element to become available.
        /// </remarks>
        /// <param name="timeout">
        ///     how long to wait before giving up, in units of
        ///     <tt>unit</tt>
        /// </param>
        /// <param name="unit">
        ///     a <tt>TimeUnit</tt> determining how to interpret the
        ///     <tt>timeout</tt> parameter
        /// </param>
        /// <returns>
        ///     the head of this queue, or <tt>null</tt> if the
        ///     specified waiting time elapses before an element is available
        /// </returns>
        /// <exception cref="System.Exception">if interrupted while waiting</exception>
        E Poll(long timeout, TimeUnit unit);

        /// <summary>Returns the number of elements in this collection.</summary>
        /// <remarks>
        ///     Returns the number of elements in this collection.  If this collection
        ///     contains more than <tt>Integer.MAX_VALUE</tt> elements, returns
        ///     <tt>Integer.MAX_VALUE</tt>.
        /// </remarks>
        /// <returns>the number of elements in this collection</returns>
        int Size();
    }
}