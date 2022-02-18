// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// The exception that is thrown when a compact serialization schema could not be found
    /// for a specified schema identifier, even after trying to fetch it from the cluster.
    /// </summary>
    [Serializable] // FIXME - what about serializable + CA1032? bonkers!
    public sealed class UnknownCompactSchemaException : SerializationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownCompactSchemaException"/> class.
        /// </summary>
        /// <param name="schemaId">The identifier of the unknown schema.</param>
        public UnknownCompactSchemaException(long schemaId)
            : base($"Unknown compact serialization schema with id {schemaId} (failed to retrieve from cluster).")
        {
            SchemaId = schemaId;
        }

        /// <summary>
        /// Gets the identifier of the unknown schema.
        /// </summary>
        public long SchemaId { get; set; }
    }
}
