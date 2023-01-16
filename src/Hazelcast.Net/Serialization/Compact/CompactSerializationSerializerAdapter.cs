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

using System;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Provides an <see cref="ISerializerAdapter"/> for the <see cref="CompactSerializationSerializer"/>.
    /// </summary>
    internal class CompactSerializationSerializerAdapter : ISerializerAdapter
    {
        private readonly CompactSerializationSerializer _serializer;
        private readonly bool _withSchemas;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactSerializerAdapter"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="withSchemas">Whether to embed all schemas in the serialized data.</param>
        public CompactSerializationSerializerAdapter(CompactSerializationSerializer serializer, bool withSchemas)
        {
            _serializer = serializer;
            _withSchemas = withSchemas;
        }

        /// <inheritdoc />
        public void Write(IObjectDataOutput output, object obj)
            => _serializer.Write(output, obj, _withSchemas);

        /// <inheritdoc />
        public object Read(IObjectDataInput input)
            => _serializer.Read(input, _withSchemas);

        /// <summary>
        /// Tries to read an object.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <param name="type">The expected type of the object.</param>
        /// <param name="obj">The object, if successful; otherwise <c>null</c>.</param>
        /// <param name="state">The state, containing the missing schema identifier, if not successful; otherwise default.</param>
        /// <returns><c>true</c> if the object was deserialized; <c>false</c> if a schema was missing..</returns>
        public bool TryRead(IObjectDataInput input, Type type, out object? obj, out SerializationService.ToObjectState state)
        {
            var result = _serializer.TryRead(input, type, _withSchemas, out obj, out var missingSchemaId);
            state = new SerializationService.ToObjectState { SchemaId = missingSchemaId };
            return result;
        } 

        /// <inheritdoc />
        public int TypeId => _withSchemas ? SerializationConstants.ConstantTypeCompactWithSchema : SerializationConstants.ConstantTypeCompact;

        /// <inheritdoc />
        public void Dispose()
            => _serializer.Dispose();

        /// <inheritdoc />
        public ISerializer Serializer => _serializer;
    }
}
