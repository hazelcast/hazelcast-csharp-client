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

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents a compact serialization schema field.
    /// </summary>
    internal class SchemaField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="kind">The field kind of the field.</param>
        public SchemaField(string name, FieldKind kind)
        {
            FieldName = name;
            Kind = kind;
        }

        // renaming these properties break the generated FieldDescriptorCodec and I have
        // not find a simple way to change the name in the generated codec, so keep these
        // names even though I would want Name and Kind.

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the field kind of the field.
        /// </summary>
        public FieldKind Kind { get; }

        // these properties are not serialized

        /// <summary>
        /// Gets the index of the reference-type field.
        /// </summary>
        internal int Index { get; set; } = -1;

        /// <summary>
        /// Gets the offset of the value-type field.
        /// </summary>
        internal int Offset { get; set; } = -1;

        /// <summary>
        /// Gets the bit-offset of the boolean value-type field.
        /// </summary>
        internal sbyte BitOffset { get; set; } = -1;
    }
}
