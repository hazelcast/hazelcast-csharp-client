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
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Data.Map;

namespace Hazelcast.DistributedObjects
{
    // TODO: see TopicEvents, all these classes must be documented

    /// <summary>
    /// Represents map event handlers.
    /// </summary>
    public sealed class MapEventHandlers<TKey, TValue> : EventHandlersBase<IMapEventHandlerBase>
    { }

    /// <summary>
    /// Specifies a generic map event handler.
    /// </summary>
    public interface IMapEventHandlerBase
    {
        /// <summary>
        /// Gets the handled event type.
        /// </summary>
        MapEventType EventType { get; }
    }

    /// <summary>
    /// Specifies a map event handler.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public interface IMapEventHandler<TKey, TValue> : IMapEventHandlerBase
    {
        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="IMap{TKey, TValue}"/> that triggered the event.</param>
        /// <param name="member">The member.</param>
        /// <param name="numberOfAffectedEntries">The number of affected entries.</param>
        void Handle(IMap<TKey, TValue> sender, MemberInfo member, int numberOfAffectedEntries);
    }

    /// <summary>
    /// Specifies a map entry event handler.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public interface IMapEntryEventHandler<TKey, TValue> : IMapEventHandlerBase
    {
        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="IMap{TKey, TValue}"/> that triggered the event.</param>
        /// <param name="member">The member.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="mergeValue">The merged value.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="numberOfAffectedEntries">The number of affected entries.</param>
        void Handle(IMap<TKey, TValue> sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries);
    }

    /// <summary>
    /// Represents event data for map events.
    /// </summary>
    public abstract class MapEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapEventArgsBase"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="numberOfAffectedEntries">The number of affected entries.</param>
        protected MapEventArgsBase(MemberInfo member, int numberOfAffectedEntries)
        {
            Member = member;
            NumberOfAffectedEntries = numberOfAffectedEntries;
        }

        /// <summary>
        /// Gets the member that originated the event.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the number of affected entries.
        /// </summary>
        public int NumberOfAffectedEntries { get; }
    }

    /// <summary>
    /// Represents event data for map entry events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    public abstract class MapEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TKey> _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapEntryEventArgsBase{TKey}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="key">The key.</param>
        protected MapEntryEventArgsBase(MemberInfo member, Lazy<TKey> key)
        {
            Member = member;
            _key = key;
        }

        /// <summary>
        /// Gets the member that originated the event.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public TKey Key => _key == null ? default : _key.Value;
    }

    /// <summary>
    /// Handles map events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TArgs">The actual type of the arguments.</typeparam>
    internal abstract class MapEventHandlerBase<TKey, TValue, TArgs> : IMapEventHandler<TKey, TValue>
    {
        private readonly Action<IMap<TKey, TValue>, TArgs> _handler;

        protected MapEventHandlerBase(MapEventType eventType, Action<IMap<TKey, TValue>, TArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventType EventType { get; }

        public void Handle(IMap<TKey, TValue> sender, MemberInfo member, int numberOfAffectedEntries)
            => _handler(sender, CreateEventArgs(member, numberOfAffectedEntries));

        protected abstract TArgs CreateEventArgs(MemberInfo member, int numberOfAffectedEntries);
    }

    /// <summary>
    /// Handles map entry events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TArgs">The actual type of the arguments.</typeparam>
    internal abstract class MapEntryEventHandlerBase<TKey, TValue, TArgs> : IMapEntryEventHandler<TKey, TValue>
    {
        private readonly Action<IMap<TKey, TValue>, TArgs> _handler;

        protected MapEntryEventHandlerBase(MapEventType eventType, Action<IMap<TKey, TValue>, TArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventType EventType { get; }

        public void Handle(IMap<TKey, TValue> sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries)
            => _handler(sender, CreateEventArgs(member, key, value, oldValue, mergeValue, eventType, numberOfAffectedEntries));

        protected abstract TArgs CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries);
    }
}