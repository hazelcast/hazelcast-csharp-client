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

using System.Threading.Tasks;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Defines a schema-management service.
    /// </summary>
    internal interface ISchemas
    {
        /// <summary>
        /// Adds a schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be already known by the cluster.</param>
        void Add(Schema schema, bool isClusterSchema);

        /// <summary>
        /// Tries to get a local schema.
        /// </summary>
        /// <param name="id">The unique identifier of the schema.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if the schema was found locally; otherwise false.</returns>
        bool TryGet(long id, out Schema schema);

        /// <summary>
        /// Gets a local or remote schema.
        /// </summary>
        /// <param name="id">The unique identifier of the schema.</param>
        /// <returns>The schema, or <c>null</c> if the schema could not be found.</returns>
        ValueTask<Schema> GetOrFetchAsync(long id);

        /// <summary>
        /// Publishes all known non-published schemas.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/>that will complete when all known non-published schemas have been published.</returns>
        ValueTask PublishAsync();
    }
}
