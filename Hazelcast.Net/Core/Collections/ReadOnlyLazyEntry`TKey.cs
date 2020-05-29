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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal class ReadOnlyLazyEntry<TValue>
    {
        private TValue _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyEntry{TValue}"/> class.
        /// </summary>
        /// <param name="valueObject">A value object.</param>
        public ReadOnlyLazyEntry(object valueObject)
        {
            if (valueObject is TValue value)
                Value = value;
            else
                ValueObject = valueObject;
        }

        /// <summary>
        /// Gets or sets the value source object.
        /// </summary>
        /// <remarks>
        /// <para>Once <see cref="Value"/> has been assigned, this property is reset to null.</para>
        /// </remarks>
        public object ValueObject { get; private set; }

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
                ValueObject = default;
            }
        }

        /// <summary>
        /// Determines whether the entry has its value already.
        /// </summary>
        public bool HasValue { get; private set; }
    }
}
