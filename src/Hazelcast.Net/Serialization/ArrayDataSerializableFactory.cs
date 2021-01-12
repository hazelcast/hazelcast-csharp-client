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

using System;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Represents an <see cref="IDataSerializableFactory"/> for arrays.
    /// </summary>
    internal class ArrayDataSerializableFactory : IDataSerializableFactory
    {
        private readonly Func<IIdentifiedDataSerializable>[] _constructors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDataSerializableFactory"/> class.
        /// </summary>
        /// <param name="constructors"></param>
        public ArrayDataSerializableFactory(Func<IIdentifiedDataSerializable>[] constructors)
        {
            _constructors = constructors;
        }

        /// <inheritdoc />
        public IIdentifiedDataSerializable Create(int typeId)
        {
            return typeId >= 0 && typeId < _constructors.Length
                ? _constructors[typeId]()
                : null;
        }
    }
}
