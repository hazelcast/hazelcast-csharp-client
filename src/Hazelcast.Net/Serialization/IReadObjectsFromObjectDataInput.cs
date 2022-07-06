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

using System;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines a service that can read objects from an <see cref="IObjectDataInput"/>.
    /// </summary>
    /// <remarks>
    /// <para>This interface exists so that we don't have to pass the full serializers
    /// in places where reading and writing objects is all we need, in order to reduce
    /// the dependency surface and simplify implementation of tests.</para>
    /// </remarks>
    internal interface IReadObjectsFromObjectDataInput
    {
        /// <summary>
        /// Reads an object.
        /// </summary>
        T Read<T>(IObjectDataInput input);

        /// <summary>
        /// Reads an object.
        /// </summary>
        object Read(IObjectDataInput input, Type type);
    }
}
