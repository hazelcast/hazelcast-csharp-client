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
using Hazelcast.Exceptions;
using Hazelcast.Serialization;

namespace Hazelcast.Projection
{
    /// <summary>
    /// Represents a simple attribute projection.
    /// </summary>
    internal class SingleAttributeProjection : IProjection, IIdentifiedDataSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAttributeProjection"/> class/.
        /// </summary>
        public SingleAttributeProjection()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAttributeProjection"/> class/.
        /// </summary>
        /// <param name="attributePath">The attribute path.</param>
        public SingleAttributeProjection(string attributePath)
        {
            if (string.IsNullOrWhiteSpace(attributePath)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(attributePath));
            AttributePath = attributePath;
        }

        /// <summary>
        /// (internal for tests only)
        /// Get the attribute path.
        /// </summary>
        internal string AttributePath { get; private set; }

        /// <inheritdoc />
        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            AttributePath = input.ReadString();
        }

        /// <inheritdoc />
        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.WriteString(AttributePath);
        }

        /// <inheritdoc />
        public int FactoryId => FactoryIds.ProjectionDsFactoryId;

        /// <inheritdoc />
        public int ClassId => ProjectionDataSerializerHook.SingleAttribute;
    }
}
