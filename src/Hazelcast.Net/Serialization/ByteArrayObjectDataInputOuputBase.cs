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

using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Provides a base class for <see cref="ByteArrayObjectDataInput"/> and <see cref="ByteArrayObjectDataOutput"/>.
    /// </summary>
    internal abstract class ByteArrayObjectDataInputOuputBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayObjectDataInputOuputBase"/> class.
        /// </summary>
        /// <param name="service">The serialization service.</param>
        /// <param name="endianness">The default endianness.</param>
        protected ByteArrayObjectDataInputOuputBase(ISerializationService service, Endianness endianness)
        {
            SerializationService = service;
            DefaultEndianness = endianness;
        }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        protected ISerializationService SerializationService { get; }

        /// <summary>
        /// Gets the default endianness.
        /// </summary>
        protected Endianness DefaultEndianness { get; }

        /// <summary>
        /// Gets the specified endianness, or the default endianness.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The specified endianness, or the default endianness.</returns>
        protected Endianness ValueOrDefault(Endianness endianness)
            => endianness == Endianness.Unspecified ? DefaultEndianness : endianness;
    }
}
