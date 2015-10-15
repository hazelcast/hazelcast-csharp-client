using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>A Ringbuffer is a data-structure where the content is stored in a ring like structure.</summary>
    /// <remarks>
    /// A Ringbuffer is a data-structure where the content is stored in a ring like structure. A ringbuffer has a capacity so it
    /// won't grow beyond that capacity and endanger the stability of the system. If that capacity is exceeded, than the oldest
    /// item in the ringbuffer is overwritten.
    /// The ringbuffer has 2 always incrementing sequences:
    /// <ol>
    /// <li>
    /// tailSequence: this is the side where the youngest item is found. So the tail is the side of the ringbuffer where
    /// items are added to.
    /// </li>
    /// <li>
    /// headSequence: this is the side where the oldest items are found. So the head is the side where items gets
    /// discarded.
    /// </li>
    /// </ol>
    /// The items in the ringbuffer can be found by a sequence that is in between (inclusive) the head and tail sequence.
    /// If data is read from a ringbuffer with a sequence that is smaller than the headSequence, it means that the data
    /// is not available anymore and a
    /// <see cref="StaleSequenceException"/>
    /// is thrown.
    /// A Ringbuffer currently is not a distributed data-structure. So all data is stored in a single partition; comparable to the
    /// IQueue implementation. But we'll provide an option to partition the data in the near future.
    /// A Ringbuffer can be used in a similar way as a queue, but one of the key differences is that a queue.take is destructive,
    /// meaning that only 1 thread is able to take an item. A ringbuffer.read is not destructive, so you can have multiple threads
    /// reading the same item multiple times.
    /// The Ringbuffer is the backing data-structure for the reliable
    /// <see cref="Hazelcast.Core.ITopic{E}"/>
    /// implementation. See
    /// <see cref="Hazelcast.Config.ReliableTopicConfig"/>
    /// .
    /// </remarks>
    /// <?/>
    public interface IRingbuffer<E> : IDistributedObject
    {
        /// <summary>Returns the capacity of this Ringbuffer.</summary>
        /// <returns>the capacity.</returns>
        long Capacity();

        /// <summary>Returns number of items in the ringbuffer.</summary>
        /// <remarks>
        /// Returns number of items in the ringbuffer.
        /// If no ttl is set, the size will always be equal to capacity after the head completed the first loop
        /// around the ring. This is because no items are getting retired.
        /// </remarks>
        /// <returns>the size.</returns>
        long Size();

        /// <summary>Returns the sequence of the tail.</summary>
        /// <remarks>
        /// Returns the sequence of the tail. The tail is the side of the ringbuffer where the items are added to.
        /// The initial value of the tail is -1.
        /// </remarks>
        /// <returns>the sequence of the tail.</returns>
        long TailSequence();

        /// <summary>Returns the sequence of the head.</summary>
        /// <remarks>
        /// Returns the sequence of the head. The head is the side of the ringbuffer where the oldest items in the
        /// ringbuffer are found.
        /// If the RingBuffer is empty, the head will be one more than the tail.
        /// The initial value of the head is 0 (1 more than tail).
        /// </remarks>
        /// <returns>the sequence of the head.</returns>
        long HeadSequence();

        /// <summary>Returns the remaining capacity of the ringbuffer.</summary>
        /// <remarks>
        /// Returns the remaining capacity of the ringbuffer.
        /// The returned value could be stale as soon as it is returned.
        /// If ttl is not set, the remaining capacity will always be the capacity.
        /// </remarks>
        /// <returns>the remaining capacity.</returns>
        long RemainingCapacity();

        /// <summary>Adds an item to the tail of the Ringbuffer.</summary>
        /// <remarks>
        /// Adds an item to the tail of the Ringbuffer. If there is no space in the Ringbuffer, the add will overwrite the oldest
        /// item in the ringbuffer no matter what the ttl is. For more control on this behavior, check the
        /// <see cref="IRingbuffer{E}.AddAsync(object, OverflowPolicy)"/>
        /// and the
        /// <see cref="OverflowPolicy"/>
        /// .
        /// The returned value is the sequence of the added item. Using this sequence you can read the added item.
        /// <h3>Using the sequence as id</h3>
        /// This sequence will always be unique for this Ringbuffer instance so it can be used as a unique id generator if you are
        /// publishing items on this Ringbuffer. However you need to take care of correctly determining an initial id when any node
        /// uses the ringbuffer for the first time. The most reliable way to do that is to write a dummy item into the ringbuffer and
        /// use the returned sequence as initial id. On the reading side, this dummy item should be discard. Please keep in mind that
        /// this id is not the sequence of the item you are about to publish but from a previously published item. So it can't be used
        /// to find that item.
        /// </remarks>
        /// <param name="item">the item to add.</param>
        /// <returns>the sequence of the added item.</returns>
        /// <exception cref="System.ArgumentNullException">if item is null.</exception>
        /// <seealso cref="Ringbuffer{E}.AddAsync(object, OverflowPolicy)"/>
        long Add(E item);

        /// <summary>
        /// Asynchronously writes an item with a configurable
        /// <see cref="OverflowPolicy"/>
        /// .
        /// If there is space in the ringbuffer, the call will return the sequence of the written item.
        /// If there is no space, it depends on the overflow policy what happens:
        /// <ol>
        /// <li>
        /// <see cref="OverflowPolicy.Overwrite"/>
        /// : we just overwrite the oldest item in the ringbuffer and we violate
        /// the ttl</li>
        /// <li>
        /// <see cref="OverflowPolicy.Fail"/>
        /// : we return -1 </li>
        /// </ol>
        /// The reason that FAIL exist is to give the opportunity to obey the ttl. If blocking behavior is required,
        /// this can be implemented using retrying in combination with a exponential backoff. Example:
        /// <code>
        /// long sleepMs = 100;
        /// for (; ; ) {
        /// long result = ringbuffer.addAsync(item, FAIL).get();
        /// if (result != -1) {
        /// break;
        /// }
        /// TimeUnit.MILLISECONDS.sleep(sleepMs);
        /// sleepMs = min(5000, sleepMs * 2);
        /// }
        /// </code>
        /// </summary>
        /// <param name="item">the item to add</param>
        /// <param name="overflowPolicy">the OverflowPolicy to use.</param>
        /// <returns>the sequenceId of the added item, or -1 if the add failed.</returns>
        /// <exception cref="System.ArgumentNullException">if item or overflowPolicy is null.</exception>
        Task<long> AddAsync(E item, OverflowPolicy overflowPolicy);

        /// <summary>Reads one item from the Ringbuffer.</summary>
        /// <remarks>
        /// Reads one item from the Ringbuffer.
        /// If the sequence is one beyond the current tail, this call blocks until an item is added.
        /// This means that the ringbuffer can be processed using the following idiom:
        /// <code>
        /// Ringbuffer&lt;String&gt; ringbuffer = hz.getRingbuffer("rb");
        /// long seq = ringbuffer.headSequence();
        /// while(true){
        /// String item = ringbuffer.readOne(seq);
        /// seq++;
        /// ... process item
        /// }
        /// </code>
        /// This method is not destructive unlike e.g. a queue.take. So the same item can be read by multiple readers or it can be
        /// read multiple times by the same reader.
        /// Currently it isn't possible to control how long this call is going to block. In the future we could add e.g.
        /// tryReadOne(long sequence, long timeout, TimeUnit unit).
        /// </remarks>
        /// <param name="sequence">the sequence of the item to read.</param>
        /// <returns>the read item</returns>
        /// <exception cref="StaleSequenceException">
        /// if the sequence is smaller then
        /// <see cref="Ringbuffer{E}.HeadSequence()"/>
        /// . Because a
        /// Ringbuffer won't store all event indefinitely, it can be that the data for the
        /// given sequence doesn't exist anymore and the
        /// <see cref="StaleSequenceException"/>
        /// is thrown. It is up to the caller to deal with this particular situation, e.g.
        /// throw an Exception or restart from the last known head. That is why the
        /// StaleSequenceException contains the last known head.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// if sequence is smaller than 0 or larger than
        /// <see cref="Ringbuffer{E}.TailSequence()"/>
        /// +1.
        /// </exception>
        /// <exception cref="System.Exception">if the call is interrupted while blocking.</exception>
        E ReadOne(long sequence);

        /// <summary>Adds all the items of a collection to the tail of the Ringbuffer.</summary>
        /// <remarks>
        /// Adds all the items of a collection to the tail of the Ringbuffer.
        /// A addAll is likely to outperform multiple calls to
        /// <see cref="Ringbuffer{E}.Add(object)"/>
        /// due to better io utilization and a reduced number
        /// of executed operations.
        /// If the batch is empty, the call is ignored.
        /// When the collection is not empty, the content is copied into a different data-structure. This means that:
        /// <ol>
        /// <li>after this call completes, the collection can be re-used.</li>
        /// <li>the collection doesn't need to be serializable</li>
        /// </ol>
        /// If the collection is larger than the capacity of the ringbuffer, then the items that were written first will
        /// be overwritten. Therefor this call will not block.
        /// The items are inserted in the order of the Iterator of the collection. If an addAll is executed concurrently with
        /// an add or addAll, no guarantee is given that items are contiguous.
        /// The result of the future contains the sequenceId of the last written item
        /// </remarks>
        /// <param name="collection">the batch of items to add.</param>
        /// <returns>the ICompletableFuture to synchronize on completion.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// if batch is null,
        /// or if an item in this batch is null
        /// or if overflowPolicy is null
        /// </exception>
        /// <exception cref="System.ArgumentException">if collection is empty</exception>
        Task<long> AddAllAsync<T>(ICollection<T> collection, OverflowPolicy overflowPolicy) where T : E;

        /// <summary>Reads a batch of items from the Ringbuffer.</summary>
        /// <remarks>
        /// Reads a batch of items from the Ringbuffer. If the number of available items after the first read item is smaller than
        /// the maxCount, these items are returned. So it could be the number of items read is smaller than the maxCount.
        /// If there are less items available than minCount, then this call blocks.
        /// Reading a batch of items is likely to perform better because less overhead is involved.
        /// </remarks>
        /// <param name="startSequence">the startSequence of the first item to read.</param>
        /// <param name="minCount">the minimum number of items to read.</param>
        /// <param name="maxCount">the maximum number of items to read.</param>
        /// <returns>a future containing the items read.</returns>
        /// <exception cref="System.ArgumentException">
        /// if startSequence is smaller than 0
        /// or if startSequence larger than
        /// <see cref="Ringbuffer{E}.TailSequence()"/>
        /// or if minCount smaller than 0
        /// or if minCount larger than maxCount,
        /// or if maxCount larger than the capacity of the ringbuffer
        /// or if maxCount larger than 1000 (to prevent overload)
        /// </exception>
        Task<IList<E>> ReadManyAsync(long startSequence, int minCount, int maxCount);
    }
}
