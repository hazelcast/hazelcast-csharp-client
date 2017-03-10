// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    /// <summary>ClassDefinition defines a class schema for Portable classes.</summary>
    /// <remarks>
    /// ClassDefinition defines a class schema for Portable classes. It allows to query field names, types, class id etc.
    /// It can be created manually using
    /// <see cref="ClassDefinitionBuilder"/>
    /// or ondemand during serialization phase.
    /// </remarks>
    /// <seealso cref="IPortable"/>
    /// <seealso cref="ClassDefinitionBuilder"/>
    public interface IClassDefinition
    {
        /// <returns>class id</returns>
        int GetClassId();

        /// <returns>factory id</returns>
        int GetFactoryId();

        /// <param name="name">name of the field</param>
        /// <returns>field definition by given name or null</returns>
        IFieldDefinition GetField(string name);

        /// <param name="fieldIndex">index of the field</param>
        /// <returns>field definition by given index</returns>
        /// <exception cref="System.IndexOutOfRangeException"/>
        IFieldDefinition GetField(int fieldIndex);

        /// <param name="fieldName">name of the field</param>
        /// <returns>class id of given field</returns>
        /// <exception cref="System.ArgumentException"/>
        int GetFieldClassId(string fieldName);

        /// <returns>total field count</returns>
        int GetFieldCount();

        /// <returns>all field names contained in this class definition</returns>
        ICollection<string> GetFieldNames();

        /// <param name="fieldName">name of the field</param>
        /// <returns>type of given field</returns>
        /// <exception cref="System.ArgumentException"/>
        FieldType GetFieldType(string fieldName);

        /// <returns>version</returns>
        int GetVersion();

        /// <param name="fieldName">field name</param>
        /// <returns>true if this class definition contains a field named by given name</returns>
        bool HasField(string fieldName);
    }
}