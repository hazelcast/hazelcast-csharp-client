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

using System;
using System.Runtime.Serialization;

// CA1032: default constructors make no sense here
#pragma warning disable CA1032

namespace Hazelcast.Serialization
{
    /// <summary>
    /// The exception that is thrown when a compact serialization schema could not be found
    /// for a specified schema identifier, even after trying to fetch it from the cluster.
    /// </summary>
    #if !NET8_0_OR_GREATER
    [Serializable]
#endif
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
#if !NET8_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownCompactSchemaException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        [Obsolete("This constructor is obsolete due to BinaryFormatter being obsolete. Use the constructor without this parameter.")]
        private UnknownCompactSchemaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SchemaId = info.GetInt64(nameof(SchemaId));
        }
#endif

        /// <summary>
        /// Gets the identifier of the unknown schema.
        /// </summary>
        public long SchemaId { get; }
#if !NET8_0_OR_GREATER
        /// <inheritdoc />
        [Obsolete("This constructor is obsolete due to BinaryFormatter being obsolete. Use the constructor without this parameter.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(SchemaId), SchemaId);
        }
#endif
    }
}
