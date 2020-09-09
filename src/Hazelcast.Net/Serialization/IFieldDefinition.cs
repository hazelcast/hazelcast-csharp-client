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

namespace Hazelcast.Serialization
{
    /// <summary>FieldDefinition defines name, type, index of a field</summary>
    public interface IFieldDefinition
    {
        /// <returns>class id of this field's class</returns>
        int ClassId { get; }

        /// <returns>factory id of this field's class</returns>
        int FactoryId { get; }

        /// <returns>field type</returns>
        FieldType FieldType { get; }

        /// <returns>field index</returns>
        int Index { get; }

        /// <returns>field name</returns>
        string Name { get; }

        /// <returns>field version</returns>
        int Version { get; }
    }
}
