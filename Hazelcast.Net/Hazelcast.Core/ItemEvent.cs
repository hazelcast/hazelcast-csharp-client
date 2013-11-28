using System;

namespace Hazelcast.Core
{
    /// <summary>Map Item event.</summary>
    /// <remarks>Map Item event.</remarks>
    /// <seealso cref="EntryEvent{K, V}">EntryEvent&lt;K, V&gt;</seealso>
    /// <seealso cref="ICollection{E}.AddListener(IItemListener{E}, bool)">
    ///     ICollection&lt;E&gt;
    ///     .AddListener(IItemListener&lt;E&gt;, bool)
    /// </seealso>
    [Serializable]
    public class ItemEvent<E> : EventObject
    {
        private readonly ItemEventType eventType;
        private readonly E item;

        private readonly IMember member;

        public ItemEvent(string name, int eventType, E item, IMember member)
            : this(name, ItemEventType.Added, item, member)
        {
        }

        public ItemEvent(string name, ItemEventType itemEventType, E item, IMember member) : base(name)
        {
            //FIXME ENUM HATASI
            //this(name, ItemEventType.getByType(eventType), item, member);
            this.item = item;
            eventType = itemEventType;
            this.member = member;
        }

        /// <summary>Returns the event type.</summary>
        /// <remarks>Returns the event type.</remarks>
        /// <returns>the event type.</returns>
        public virtual ItemEventType GetEventType()
        {
            return eventType;
        }

        /// <summary>Returns the item related to event.</summary>
        /// <remarks>Returns the item related to event.</remarks>
        /// <returns>the item.</returns>
        public virtual E GetItem()
        {
            return item;
        }

        /// <summary>Returns the member fired this event.</summary>
        /// <remarks>Returns the member fired this event.</remarks>
        /// <returns>the member fired this event.</returns>
        public virtual IMember GetMember()
        {
            return member;
        }

        public override string ToString()
        {
            return "ItemEvent{" + "event=" + eventType + ", item=" + GetItem() + ", member=" + GetMember() + "} ";
        }
    }
}