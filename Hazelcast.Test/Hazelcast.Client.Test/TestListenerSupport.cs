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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Core;

namespace Hazelcast.Client.Test
{
    public class EntryListener<K, V> : IEntryListener<K, V>
    {
        public Action<EntryEvent<K, V>> EntryAddedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryUpdatedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryRemovedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryEvictedAction { get; set; }
        public Action<MapEvent> MapEvictedAction { get; set; }
        public Action<MapEvent> MapClearedAction { get; set; }

        public void EntryAdded(EntryEvent<K, V> @event)
        {
            if (EntryAddedAction != null)
            {
                EntryAddedAction(@event);
            }
        }

        public void EntryRemoved(EntryEvent<K, V> @event)
        {
            if (EntryRemovedAction != null)
            {
                EntryRemovedAction(@event);
            }
        }

        public void EntryUpdated(EntryEvent<K, V> @event)
        {
            if (EntryUpdatedAction != null)
            {
                EntryUpdatedAction(@event);
            }
        }

        public void EntryEvicted(EntryEvent<K, V> @event)
        {
            if (EntryEvictedAction != null)
            {
                EntryEvictedAction(@event);
            }
        }

        public void MapEvicted(MapEvent @event)
        {
            if (MapEvictedAction != null)
            {
                MapEvictedAction(@event);
            }
        }

        public void MapCleared(MapEvent @event)
        {
            if (MapClearedAction != null)
            {
                MapClearedAction(@event);
            }
        }
    }

    public class MembershipListener : IMembershipListener
    {
        public Action<MembershipEvent> MemberAddedAction { get; set; }
        public Action<MembershipEvent> MemberRemovedAction { get; set; }
        public Action<MemberAttributeEvent> MemberAttributeChangedAction { get; set; }
        public void MemberAdded(MembershipEvent membershipEvent)
        {
            if (MemberAddedAction != null) MemberAddedAction(membershipEvent);
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            if (MemberRemovedAction != null) MemberRemovedAction(membershipEvent);
        }

        public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
        {
            if (MemberAttributeChangedAction != null) 
                MemberAttributeChangedAction(memberAttributeEvent);
        }
    }
}
