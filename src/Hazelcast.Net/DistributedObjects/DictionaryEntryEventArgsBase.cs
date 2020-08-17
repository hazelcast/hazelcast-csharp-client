﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for map entry events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    public abstract class DictionaryEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TKey> _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryEntryEventArgsBase{TKey}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="key">The key.</param>
        protected DictionaryEntryEventArgsBase(MemberInfo member, Lazy<TKey> key)
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
}
