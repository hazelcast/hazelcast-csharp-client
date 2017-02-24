// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    /// <summary>
    ///     ClassDefinitionBuilder is used to build and register ClassDefinitions manually.
    /// </summary>
    /// <remarks>
    ///     ClassDefinitionBuilder is used to build and register ClassDefinitions manually.
    /// </remarks>
    /// <seealso cref="IClassDefinition">IClassDefinition</seealso>
    /// <seealso cref="IPortable">IPortable</seealso>
    /// <seealso cref="Hazelcast.Config.SerializationConfig.AddClassDefinition(IClassDefinition)">
    ///     Hazelcast.Config.SerializationConfig.AddClassDefinition(IClassDefinition)
    /// </seealso>
    public sealed class ClassDefinitionBuilder
    {
        private readonly int _classId;
        private readonly int _factoryId;

        private readonly IList<FieldDefinition> _fieldDefinitions = new List<FieldDefinition>();
        private readonly int _version;

        private bool _done;
        private int _index;

        public ClassDefinitionBuilder(int factoryId, int classId)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = -1;
        }

        public ClassDefinitionBuilder(int factoryId, int classId, int version)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = version;
        }

        public ClassDefinitionBuilder AddBooleanArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.BooleanArray));
            return this;
        }

        public ClassDefinitionBuilder AddBooleanField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Boolean));
            return this;
        }

        public ClassDefinitionBuilder AddByteArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.ByteArray));
            return this;
        }

        public ClassDefinitionBuilder AddByteField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Byte));
            return this;
        }

        public ClassDefinitionBuilder AddCharArrayField(string
            fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.CharArray));
            return this;
        }

        public ClassDefinitionBuilder AddCharField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Char));
            return this;
        }

        public ClassDefinitionBuilder AddDoubleArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.DoubleArray));
            return this;
        }

        public ClassDefinitionBuilder AddDoubleField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Double));
            return this;
        }

        public ClassDefinitionBuilder AddFloatArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.FloatArray));
            return this;
        }

        public ClassDefinitionBuilder AddFloatField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Float));
            return this;
        }

        public ClassDefinitionBuilder AddIntArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.IntArray));
            return this;
        }

        public ClassDefinitionBuilder AddIntField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Int));
            return this;
        }

        public ClassDefinitionBuilder AddLongArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.LongArray));
            return this;
        }

        public ClassDefinitionBuilder AddLongField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Long));
            return this;
        }

        public ClassDefinitionBuilder AddPortableArrayField(string fieldName, IClassDefinition def)
        {
            Check();
            if (def.GetClassId() == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.PortableArray, def.GetFactoryId(),
                def.GetClassId()));
            return this;
        }

        public ClassDefinitionBuilder AddPortableField(string fieldName, IClassDefinition def)
        {
            Check();
            if (def.GetClassId() == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Portable, def.GetFactoryId(),
                def.GetClassId()));
            return this;
        }

        public ClassDefinitionBuilder AddShortArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.ShortArray));
            return this;
        }

        public ClassDefinitionBuilder AddShortField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Short));
            return this;
        }

        public ClassDefinitionBuilder AddUTFArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.UtfArray));
            return this;
        }

        public ClassDefinitionBuilder AddUTFField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Utf));
            return this;
        }

        public IClassDefinition Build()
        {
            _done = true;
            var cd = new ClassDefinition(_factoryId, _classId, _version);
            foreach (var fd in _fieldDefinitions)
            {
                cd.AddFieldDef(fd);
            }
            return cd;
        }

        public int GetClassId()
        {
            return _classId;
        }

        public int GetFactoryId()
        {
            return _factoryId;
        }

        public int GetVersion()
        {
            return _version;
        }

        internal ClassDefinitionBuilder AddField(FieldDefinition fieldDefinition)
        {
            Check();
            if (_index != fieldDefinition.GetIndex())
            {
                throw new ArgumentException("Invalid field index");
            }
            _index++;
            _fieldDefinitions.Add(fieldDefinition);
            return this;
        }

        private void Check()
        {
            if (_done)
            {
                throw new HazelcastSerializationException("ClassDefinition is already built for " + _classId);
            }
        }
    }
}