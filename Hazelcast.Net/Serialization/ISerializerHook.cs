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

using System;

namespace Hazelcast.Serialization
{
    public interface ISerializerHook<T>
    {
        /// <summary>Creates a new serializer for the serialization type</summary>
        /// <returns>a new serializer instance</returns>
        ISerializer CreateSerializer();

        /// <summary>Returns the actual class type of the serialized object</summary>
        /// <returns>the serialized object type</returns>
        Type GetSerializationType();

        /// <summary>
        /// Defines if this serializer can be overridden by defining a custom
        /// serializer in the configurations (codebase or configuration file)
        /// </summary>
        /// <returns>if the serializer is overwritable</returns>
        bool IsOverwritable();
    }
}