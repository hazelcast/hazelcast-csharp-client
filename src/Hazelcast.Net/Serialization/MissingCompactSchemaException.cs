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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// The exception that is thrown when a compact serialization schema could not be found
    /// for a specified schema identifier, 
    /// </summary>
    [Serializable] // FIXME - what about serializable + CA1032? bonkers!
    public sealed class MissingCompactSchemaException : SerializationException
    {
        private readonly Func<long, ValueTask<Schema?>> _fetch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingCompactSchemaException"/> class.
        /// </summary>
        /// <param name="schemaId">The identifier of the missing schema.</param>
        /// <param name="fetchAsync">A method that can be used to fetch the missing schema from the cluster.</param>
        public MissingCompactSchemaException(long schemaId, Func<long, ValueTask<Schema?>> fetchAsync)
            : base($"Missing compact serialization schema with id {schemaId} (try to retrieve from cluster).")
        {
            SchemaId = schemaId;
            _fetch = fetchAsync;
        }

        /// <summary>
        /// Gets the identifier of the missing schema.
        /// </summary>
        public long SchemaId { get; set; }

        /// <summary>
        /// Asynchronously fetches the missing schema from the cluster.
        /// </summary>
        public async Task FetchSchemaAsync()
        {
            var schema = await _fetch(SchemaId).CfAwait();
            if (schema == null) throw new UnknownCompactSchemaException(SchemaId);
        }
    }
}
