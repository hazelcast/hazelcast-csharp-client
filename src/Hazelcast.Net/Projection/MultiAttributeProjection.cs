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

using System;
using Hazelcast.Serialization;

namespace Hazelcast.Projection
{
    /// <summary>
    /// Represents a multiple attributes projection.
    /// </summary>
    internal class MultiAttributeProjection : IProjection, IIdentifiedDataSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiAttributeProjection"/> class.
        /// </summary>
        public MultiAttributeProjection()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiAttributeProjection"/> class.
        /// </summary>
        /// <param name="attributePaths">The attribute paths.</param>
        public MultiAttributeProjection(string[] attributePaths)
        {
            if (attributePaths == null) throw new ArgumentNullException(nameof(attributePaths));

            foreach (var attributePath in attributePaths)
            {
                if (string.IsNullOrWhiteSpace(attributePath))
                    throw new ArgumentException("No attribute path can be null nor empty.", nameof(attributePaths));
#if NETFRAMEWORK
                if (attributePath.Contains("[any]"))
#else
                if (attributePath.Contains("[any]", StringComparison.OrdinalIgnoreCase))
#endif
                    throw new ArgumentException("No attribute path can contain the '[any]' operator.", nameof(attributePaths));
            }

            AttributePaths = attributePaths;
        }

        /// <summary>
        /// (internal for tests only)
        /// Get the attribute path.
        /// </summary>
        internal string[] AttributePaths { get; private set; }

        /// <inheritdoc />
        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            AttributePaths = input.ReadStringArray();
        }

        /// <inheritdoc />
        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.WriteStringArray(AttributePaths);
        }

        /// <inheritdoc />
        public int FactoryId => FactoryIds.ProjectionDsFactoryId;

        /// <inheritdoc />
        public int ClassId => ProjectionDataSerializerHook.MultiAttribute;
    }
}
