// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    [Serializable]
    public class DataAwareEntryEvent<TKey, TValue> : EntryEvent<TKey, TValue>
    {
        [NonSerialized] private readonly Lazy<TKey> _key;
        [NonSerialized] private readonly Lazy<TValue> _value;
        [NonSerialized] private readonly Lazy<TValue> _oldValue;
        [NonSerialized] private readonly Lazy<TValue> _mergingValue;

        [NonSerialized] private readonly ISerializationService _serializationService;

        public DataAwareEntryEvent(string source, IMember member, EntryEventType eventType, IData keyData,
            IData valueData, IData oldValueData, IData mergingValueData, ISerializationService serializationService)
            : base(source, member, eventType, default(TKey), default(TValue), default(TValue), default(TValue))
        {
            _serializationService = serializationService;
            _key = new Lazy<TKey>(() => ValueFactory<TKey>(keyData));
            _value = new Lazy<TValue>(() => ValueFactory<TValue>(valueData));
            _oldValue = new Lazy<TValue>(() => ValueFactory<TValue>(oldValueData));
            _mergingValue = new Lazy<TValue>(() => ValueFactory<TValue>(mergingValueData));
        }
        
        public override TKey GetKey()
        {
            return _key.Value;
        }

        public override TValue GetOldValue()
        {
            return _oldValue.Value;
        }

        public override TValue GetValue()
        {
            return _value.Value;
        }

        public override TValue GetMergingValue()
        {
            return _mergingValue.Value;
        }

        private TOut ValueFactory<TOut>(object input)
        {
            return _serializationService.ToObject<TOut>(input);
        }
    }

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
        private readonly TValue _value;
        private readonly TValue _oldValue;
        private readonly TValue _mergingValue;

        public EntryEvent(string source, IMember member, EntryEventType eventType, TKey key, TValue value,
            TValue oldValue = default(TValue), TValue mergingValue = default(TValue))
            : base(source, member, eventType)
        {
            _key = key;
            _value = value;
            _oldValue = oldValue;
            _mergingValue = mergingValue;
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

        /// <summary>Returns the value of the entry event</summary>
        /// <returns>the value</returns>
        public virtual TValue GetValue()
        {
            return _value;
        }

        /// <summary>Returns the incoming merging value of the entry event.</summary>
        /// <returns>merge value</returns>
        public virtual TValue GetMergingValue()
        {
            return _mergingValue;
        }

        public override string ToString()
        {
            return string.Format("EntryEvent{ {0}, key={1}, oldValue={2}, value={3}, mergingValue={4} }",
                base.ToString(), GetKey(), GetOldValue(), GetValue(), GetMergingValue());
        }
    }

    public abstract class AbstractMapEvent : EventObject
    {
        private readonly EntryEventType _entryEventType;

        protected internal readonly IMember Member;
        protected internal readonly string Name;

        protected AbstractMapEvent(string source, IMember member, EntryEventType eventType)
            : base(source)
        {
            Name = source;
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
        /// <returns>the member fired this event.</returns>
        public virtual IMember GetMember()
        {
            return Member;
        }

        /// <summary>Returns the name of the map for this event.</summary>
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
            return string.Format("entryEventType={0}, member={1}, name={2}", _entryEventType, Member, GetName());
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