using System;

namespace Hazelcast.Core
{
    /// <summary>Map Entry event.</summary>
    /// <remarks>Map Entry event.</remarks>
    /// <?></?>
    /// <?></?>
    /// 
    /// <seealso cref="IEntryListener{K,V}">IEntryListener&lt;K, V&gt;</seealso>
    /// <seealso cref="IMap{K, V}.AddEntryListener(IEntryListener{K,V}, bool)">
    ///     IMap&lt;K, V&gt;.AddEntryListener(IEntryListener
    ///     &lt;K, V&gt;, bool)
    /// </seealso>
    [Serializable]
    public class EntryEvent<K, V> : EventObject
    {
        protected internal readonly EntryEventType entryEventType;
        protected internal readonly IMember member;

        protected internal readonly string name;

        protected internal K key;

        protected internal V oldValue;

        protected internal V value;

        public EntryEvent(object source, IMember member, EntryEventType eventType, K key, V value)
            : this(source, member, eventType, key, default(V), value)
        {
        }

        public EntryEvent(object source, IMember member, EntryEventType eventType, K key, V oldValue, V value)
            : base(source)
        {
            name = (string) source;
            this.member = member;
            this.key = key;
            this.oldValue = oldValue;
            this.value = value;
            entryEventType = eventType;
        }

        //.getByType(eventType);
        public override object GetSource()
        {
            return name;
        }

        /// <summary>Returns the key of the entry event</summary>
        /// <returns>the key</returns>
        public virtual K GetKey()
        {
            return key;
        }

        /// <summary>Returns the old value of the entry event</summary>
        /// <returns></returns>
        public virtual V GetOldValue()
        {
            return oldValue;
        }

        /// <summary>Returns the value of the entry event</summary>
        /// <returns></returns>
        public virtual V GetValue()
        {
            return value;
        }

        /// <summary>Returns the member fired this event.</summary>
        /// <remarks>Returns the member fired this event.</remarks>
        /// <returns>the member fired this event.</returns>
        public virtual IMember GetMember()
        {
            return member;
        }

        /// <summary>Return the event type</summary>
        /// <returns>event type</returns>
        public virtual EntryEventType GetEventType()
        {
            return entryEventType;
        }

        /// <summary>Returns the name of the map for this event.</summary>
        /// <remarks>Returns the name of the map for this event.</remarks>
        /// <returns>name of the map.</returns>
        public virtual string GetName()
        {
            return name;
        }

        public override string ToString()
        {
            return "EntryEvent {" + GetSource() + "} key=" + GetKey() + ", oldValue=" + GetOldValue() + ", value=" +
                   GetValue() + ", event=" + entryEventType + ", by " + member;
        }
    }

    public class EventObject
    {
        [NonSerialized] internal object Source;

        public EventObject(Object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
        }

        public virtual Object GetSource()
        {
            return Source;
        }
    }
}