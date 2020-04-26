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

namespace Hazelcast.Core.Collections
{
    /// <summary>
    /// Represents an entry in a lazy collection.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TSource">The type of the source values.</typeparam>
    internal class CacheEntry<TValue, TSource>
    {
        private TValue _value;

        /// <summary>
        /// Gets or sets the source value.
        /// </summary>
        /// <remarks>
        /// <para>Once <see cref="Value"/> has been assigned, this property is reset to default.</para>
        /// </remarks>
        public TSource Source { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TValue Value
        {
            get => _value;
            set
            {
                _value = value;
                HasValue = true;
                Source = default;
            }
        }

        /// <summary>
        /// Determines whether the entry has its value already.
        /// </summary>
        public bool HasValue { get; private set; }
    }
}