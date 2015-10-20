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
    /// <summary>Map Item event.</summary>
    /// <remarks>Map Item event.</remarks>
    /// <seealso cref="EntryEvent{K, V}">EntryEvent&lt;K, V&gt;</seealso>
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