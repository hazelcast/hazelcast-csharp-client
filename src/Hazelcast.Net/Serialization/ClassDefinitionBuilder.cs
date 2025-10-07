// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    /// <summary>
    ///     ClassDefinitionBuilder is used to build and register ClassDefinitions manually.
    /// </summary>
    /// <remarks>
    ///     ClassDefinitionBuilder is used to build and register ClassDefinitions manually.
    /// </remarks>
    /// <seealso cref="IClassDefinition">IClassDefinition</seealso>
    /// <seealso cref="IPortable">IPortable</seealso>
    /// <seealso cref="SerializationOptions.AddClassDefinition(IClassDefinition)">
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

        /// <summary>
        ///     Creates a new ClassDefinitionBuilder
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="classId"></param>
        public ClassDefinitionBuilder(int factoryId, int classId)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = 0;
        }

        /// <summary>
        ///     Creates a new ClassDefinitionBuilder
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="classId"></param>
        /// <param name="version"></param>
        public ClassDefinitionBuilder(int factoryId, int classId, int version)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = version;
        }

        /// <summary>
        ///     Adds a boolean array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddBooleanArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.BooleanArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a boolean field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddBooleanField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Boolean, _version));
            return this;
        }

        /// <summary>
        ///     Adds a byte array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddByteArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.ByteArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a byte field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddByteField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Byte, _version));
            return this;
        }

        /// <summary>
        ///     Adds a char array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddCharArrayField(string
            fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.CharArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a char field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddCharField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Char, _version));
            return this;
        }

        /// <summary>
        ///     Adds a double array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddDoubleArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.DoubleArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a double field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddDoubleField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Double, _version));
            return this;
        }

        /// <summary>
        ///     Adds a float array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddFloatArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.FloatArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a float field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddFloatField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Float, _version));
            return this;
        }

        /// <summary>
        ///     Adds an int array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddIntArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.IntArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds an int field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddIntField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Int, _version));
            return this;
        }

        /// <summary>
        ///     Adds a long array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddLongArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.LongArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a long field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddLongField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Long, _version));
            return this;
        }

        /// <summary>
        ///     Adds a portable array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ClassDefinitionBuilder AddPortableArrayField(string fieldName, IClassDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            Check();
            if (def.ClassId == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.PortableArray, def.FactoryId,
                def.ClassId, def.Version));
            return this;
        }

        /// <summary>
        ///     Adds a portable field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ClassDefinitionBuilder AddPortableField(string fieldName, IClassDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            Check();
            if (def.ClassId == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Portable, def.FactoryId,
                def.ClassId, def.Version));
            return this;
        }

        /// <summary>
        ///     Adds a short array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddShortArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.ShortArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a short field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddShortField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Short, _version));
            return this;
        }

        /// <summary>
        ///     Adds a string array field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddStringArrayField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.UtfArray, _version));
            return this;
        }

        /// <summary>
        ///     Adds a string field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public ClassDefinitionBuilder AddStringField(string fieldName)
        {
            Check();
            _fieldDefinitions.Add(new FieldDefinition(_index++, fieldName, FieldType.Utf, _version));
            return this;
        }

        /// <summary>
        ///     Builds the class definition
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Gets the class id
        /// </summary>
        public int ClassId => _classId;

        /// <summary>
        ///     Gets the factory id
        /// </summary>
        public int FactoryId => _factoryId;

        /// <summary>
        ///     Gets the version
        /// </summary>
        public int Version => _version;

        internal ClassDefinitionBuilder AddField(FieldDefinition fieldDefinition)
        {
            Check();
            if (_index != fieldDefinition.Index)
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
                throw new SerializationException("ClassDefinition is already built for " + _classId);
            }
        }
    }
}
