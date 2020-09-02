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

namespace Hazelcast.Serialization.Collections
{
    /// <summary>
    /// Represents an entry in a lazy collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal class ReadOnlyLazyEntry<TKey, TValue> : ReadOnlyLazyEntry<TValue>
    {
        private TKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueData">The value data.</param>
        public ReadOnlyLazyEntry(TKey key, IData valueData)
            : base(valueData)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="value">The value.</param>
        public ReadOnlyLazyEntry(IData keyData, TValue value)
            : base(value)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public ReadOnlyLazyEntry(TKey key, TValue value)
            : base(value)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueData">The value data.</param>
        public ReadOnlyLazyEntry(IData keyData, IData valueData)
            : base(valueData)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Gets or sets the key source data.
        /// </summary>
        /// <remarks>
        /// <para>Once <see cref="Key"/> has been assigned, this property is reset to null.</para>
        /// </remarks>
        public IData KeyData { get; private set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public TKey Key
        {
            get => _key;
            set
            {
                _key = value;
                HasKey = true;
                KeyData = default;
            }
        }

        /// <summary>
        /// Determines whether the entry has its key already.
        /// </summary>
        public bool HasKey { get; private set; }
    }
}
