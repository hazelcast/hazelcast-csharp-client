// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Specifies a map entry event handler.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    public interface IMapEntryEventHandler<TKey, TValue, in TSender> : IMapEventHandlerBase
    {
        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="sender">The sender (map) that triggered the event.</param>
        /// <param name="member">The member.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="mergeValue">The merged value.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="numberOfAffectedEntries">The number of affected entries.</param>
        /// <param name="state">A state object.</param>
        ValueTask HandleAsync(TSender sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventTypes eventType, int numberOfAffectedEntries, object state);
    }
}
