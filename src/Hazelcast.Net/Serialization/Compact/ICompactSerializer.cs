// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    /// Defines a compact serializer.
    /// </summary>
    public interface ICompactSerializer
    {
        /// <summary>
        /// Gets the schema type name.
        /// </summary>
        string TypeName { get; }
    }

    /// <summary>
    /// Defines a compact serializer for a specified type.
    /// </summary>
    /// <typeparam name="T">The serialized type.</typeparam>
    public interface ICompactSerializer<T> : ICompactSerializer where T : notnull
    {
        /// <summary>
        /// Reads a value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The value.</returns>
        T Read(ICompactReader reader);

        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        void Write(ICompactWriter writer, T value);
    }
}
