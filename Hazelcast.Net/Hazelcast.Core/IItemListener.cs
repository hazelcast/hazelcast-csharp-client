namespace Hazelcast.Core
{
    /// <summary>
    ///     Item listener for
    ///     <see cref="IQueue{E}"/>
    ///     ,
    ///     <see cref="IHSet{E}"/>
    ///     and
    ///     <see cref="IHList{E}"/>
    /// </summary>
    public interface IItemListener<E> : IEventListener
    {
        /// <summary>Invoked when an item is added.</summary>
        /// <remarks>Invoked when an item is added.</remarks>
        /// <param name="item">added item</param>
        void ItemAdded(ItemEvent<E> item);

        /// <summary>Invoked when an item is removed.</summary>
        /// <remarks>Invoked when an item is removed.</remarks>
        /// <param name="item">removed item.</param>
        void ItemRemoved(ItemEvent<E> item);
    }
}