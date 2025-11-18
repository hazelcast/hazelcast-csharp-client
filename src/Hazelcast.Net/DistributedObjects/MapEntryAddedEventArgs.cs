// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a map entry added event arguments.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class MapEntryAddedEventArgs<TKey, TValue> : MapEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TValue> _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapEntryAddedEventArgs{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        public MapEntryAddedEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, object state)
            : base(member, key, state)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the added value.
        /// </summary>
        public TValue Value => _value == null ? default : _value.Value;
    }
}
