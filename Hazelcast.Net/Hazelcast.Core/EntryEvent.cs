/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;

namespace Hazelcast.Core
{
    /// <summary>Map Entry event.</summary>
    /// <remarks>Map Entry event.</remarks>
    /// <typeparam name="TKey">type of key</typeparam>
    /// <typeparam name="TValue">type of value</typeparam>
    /// <seealso cref="IEntryListener{TKey,TValue}" />
    /// <seealso cref="IMap{TKey,TValue}.AddEntryListener(IEntryListener{TKey,TValue}, bool)" />
    [Serializable]
    public class EntryEvent<TKey, TValue> : AbstractMapEvent
    {
        private TKey key;
        private TValue oldValue;
        private TValue value;

        public EntryEvent(object source, IMember member, EntryEventType eventType, TKey key, TValue value)
            : this(source, member, eventType, key, default(TValue), value)
        {
        }

        public EntryEvent(object source, IMember member, EntryEventType eventType, TKey key, TValue oldValue, TValue value)
            : base(source, member,eventType)
        {
            this.key = key;
            this.oldValue = oldValue;
            this.value = value;
        }

        public override object GetSource()
        {
            return name;
        }

        /// <summary>Returns the key of the entry event</summary>
        /// <returns>the key</returns>
        public virtual TKey GetKey()
        {
            return key;
        }

        /// <summary>Returns the old value of the entry event</summary>
        /// <returns>the old value</returns>
        public virtual TValue GetOldValue()
        {
            return oldValue;
        }

        /// <summary>Returns the value of the entry event</summary>
        /// <returns>the valueS</returns>
        public virtual TValue GetValue()
        {
            return value;
        }

        public override string ToString()
        {
            return "EntryEvent {" + GetSource() + "} key=" + GetKey() + ", oldValue=" + GetOldValue() + ", value=" +
                   GetValue() + ", event=" + GetEventType() + ", by " + member;
        }
    }

    public abstract class AbstractMapEvent : EventObject
    {

        protected internal readonly string name;
        private readonly EntryEventType entryEventType;
        protected internal readonly IMember member;

        protected AbstractMapEvent(object source, IMember member, EntryEventType eventType)
            : base(source)
        {
            name = (string) source;
            this.member = member;
            entryEventType = eventType;
        }

        public override object GetSource()
        {
            return name;
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
            return "AbstractMapEvent {" + GetName() + "} eventType=" + entryEventType + ", by " + member;
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

        /// <summary>
        /// The object on which the Event initially occurred.
        /// </summary>
        /// <returns>The object on which the Event initially occurred.</returns>
        public virtual Object GetSource()
        {
            return Source;
        }
    }
}