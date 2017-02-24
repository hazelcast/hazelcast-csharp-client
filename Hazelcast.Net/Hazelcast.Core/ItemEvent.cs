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
    /// <summary>Map Item event.</summary>
    /// <remarks>Map Item event.</remarks>
    /// <seealso cref="EntryEvent{TKey, TValue}" />
    [Serializable]
    public class ItemEvent<TE> : EventObject
    {
        private readonly ItemEventType _eventType;
        private readonly TE _item;

        private readonly IMember _member;

        public ItemEvent(string name, ItemEventType itemEventType, TE item, IMember member) : base(name)
        {
            _item = item;
            _eventType = itemEventType;
            _member = member;
        }

        /// <summary>Returns the event type.</summary>
        /// <remarks>Returns the event type.</remarks>
        /// <returns>the event type.</returns>
        public virtual ItemEventType GetEventType()
        {
            return _eventType;
        }

        /// <summary>Returns the item related to event.</summary>
        /// <remarks>Returns the item related to event.</remarks>
        /// <returns>the item.</returns>
        public virtual TE GetItem()
        {
            return _item;
        }

        /// <summary>Returns the member fired this event.</summary>
        /// <remarks>Returns the member fired this event.</remarks>
        /// <returns>the member fired this event.</returns>
        public virtual IMember GetMember()
        {
            return _member;
        }

        public override string ToString()
        {
            return "ItemEvent{" + "event=" + _eventType + ", item=" + GetItem() + ", member=" + GetMember() + "} ";
        }
    }
}