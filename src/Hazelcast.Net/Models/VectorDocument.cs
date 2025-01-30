// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.Models
{
    /// <summary>
    /// Represents a document containing a value and associated vector values.
    /// </summary>
    /// <typeparam name="TVal">The type of the value contained in the document.</typeparam>
    public class VectorDocument<TVal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorDocument{TVal}"/> class.
        /// </summary>
        private VectorDocument() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorDocument{TVal}"/> class with the specified value and vector values.
        /// </summary>
        /// <param name="value">The value contained in the document.</param>
        /// <param name="vectorValues">The vector values associated with the document.</param>
        public VectorDocument(TVal value, VectorValues vectorValues)
        {
            Value = value;
            Vectors = vectorValues;
        }

        /// <summary>
        /// Gets the value contained in the document.
        /// </summary>
        public TVal Value { get; }

        /// <summary>
        /// Gets the vector values associated with the document.
        /// </summary>
        public VectorValues Vectors { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="VectorDocument{TVal}"/> class with the specified value and vector values.
        /// </summary>
        /// <param name="value">The value contained in the document.</param>
        /// <param name="vectorValues">The vector values associated with the document.</param>
        /// <returns>A new instance of the <see cref="VectorDocument{TVal}"/> class.</returns>
        public static VectorDocument<TVal> Of(TVal value, VectorValues vectorValues)
            => new VectorDocument<TVal>(value, vectorValues);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var that = (VectorDocument<TVal>) obj;
            return Equals(Value, that.Value) && Equals(Vectors, that.Vectors);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Vectors);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "VectorDocument{"
                   + "value=" + Value
                   + ", vectors=" + Vectors
                   + '}'
                ;
        }
    }
}
