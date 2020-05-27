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

using Hazelcast.Serialization;

namespace Hazelcast.Core.Collections
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
        /// <param name="key">A key.</param>
        /// <param name="valueObject">A value object.</param>
        public ReadOnlyLazyEntry(TKey key, object valueObject)
            : base(valueObject)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="key">A key data.</param>
        /// <param name="valueObject">A value object.</param>
        public ReadOnlyLazyEntry(IData key, object valueObject)
            : base(valueObject)
        {
            KeyData = key;
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