// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        private readonly TKey _key;
        private readonly TValue _oldValue;
        private readonly TValue _value;

        public EntryEvent(object source, IMember member, EntryEventType eventType, TKey key, TValue value)
            : this(source, member, eventType, key, default(TValue), value)
        {
        }

        public EntryEvent(object source, IMember member, EntryEventType eventType, TKey key, TValue oldValue,
            TValue value)
            : base(source, member, eventType)
        {
            _key = key;
            _oldValue = oldValue;
            _value = value;
        }

        /// <summary>Returns the key of the entry event</summary>
        /// <returns>the key</returns>
        public virtual TKey GetKey()
        {
            return _key;
        }

        /// <summary>Returns the old value of the entry event</summary>
        /// <returns>the old value</returns>
        public virtual TValue GetOldValue()
        {
            return _oldValue;
        }

        public override object GetSource()
        {
            return Name;
        }

        /// <summary>Returns the value of the entry event</summary>
        /// <returns>the valueS</returns>
        public virtual TValue GetValue()
        {
            return _value;
        }

        public override string ToString()
        {
            return "EntryEvent {" + GetSource() + "} key=" + GetKey() + ", oldValue=" + GetOldValue() + ", value=" +
                   GetValue() + ", event=" + GetEventType() + ", by " + Member;
        }
    }

    public abstract class AbstractMapEvent : EventObject
    {
        private readonly EntryEventType _entryEventType;

        protected internal readonly IMember Member;
        protected internal readonly string Name;

        protected AbstractMapEvent(object source, IMember member, EntryEventType eventType)
            : base(source)
        {
            Name = (string) source;
            Member = member;
            _entryEventType = eventType;
        }

        /// <summary>Return the event type</summary>
        /// <returns>event type</returns>
        public virtual EntryEventType GetEventType()
        {
            return _entryEventType;
        }

        /// <summary>Returns the member fired this event.</summary>
        /// <remarks>Returns the member fired this event.</remarks>
        /// <returns>the member fired this event.</returns>
        public virtual IMember GetMember()
        {
            return Member;
        }

        /// <summary>Returns the name of the map for this event.</summary>
        /// <remarks>Returns the name of the map for this event.</remarks>
        /// <returns>name of the map.</returns>
        public virtual string GetName()
        {
            return Name;
        }

        public override object GetSource()
        {
            return Name;
        }

        public override string ToString()
        {
            return "AbstractMapEvent {" + GetName() + "} eventType=" + _entryEventType + ", by " + Member;
        }
    }

    public class EventObject
    {
        [NonSerialized] internal object Source;

        public EventObject(object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
        }

        /// <summary>
        /// The object on which the Event initially occurred.
        /// </summary>
        /// <returns>The object on which the Event initially occurred.</returns>
        public virtual object GetSource()
        {
            return Source;
        }
    }
}