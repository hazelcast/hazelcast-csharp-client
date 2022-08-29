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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Serialization
{
    internal class ClassDefinition : IClassDefinition
    {
        private readonly int _classId;
        private readonly int _factoryId;

        private readonly IDictionary<string, IFieldDefinition> _fieldDefinitionsMap =
            new Dictionary<string, IFieldDefinition>();

        private int _version = -1;

        public ClassDefinition()
        { }

        public ClassDefinition(int factoryId, int classId, int version)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = version;
        }

        public virtual IFieldDefinition GetField(string name)
        {
            return _fieldDefinitionsMap.TryGetValue(name, out var val) ? val : null;
        }

        public virtual IFieldDefinition GetField(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= _fieldDefinitionsMap.Count)
                throw new ArgumentOutOfRangeException(nameof(fieldIndex), $"Cannot get field with index {fieldIndex}, map contains {_fieldDefinitionsMap.Count} fields.");

            var fieldDefinition = _fieldDefinitionsMap.Values.FirstOrDefault(x => x.Index == fieldIndex);
            if (fieldDefinition != null) return fieldDefinition;

            throw new InvalidOperationException($"Failed to find field with index {fieldIndex} in map containing {_fieldDefinitionsMap.Count} fields.");
        }

        public virtual bool HasField(string fieldName)
        {
            return _fieldDefinitionsMap.ContainsKey(fieldName);
        }

        public virtual ICollection<string> GetFieldNames()
        {
            return new HashSet<string>(_fieldDefinitionsMap.Keys);
        }

        public virtual FieldType GetFieldType(string fieldName)
        {
            var fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.FieldType;
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            var fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.ClassId;
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldCount()
        {
            return _fieldDefinitionsMap.Count;
        }

        public int FactoryId => _factoryId;

        public int ClassId => _classId;

        public int Version => _version;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ClassDefinition) obj);
        }

        public override int GetHashCode()
        {
            var result = _classId;
            result = 31*result + _version;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ClassDefinition");
            sb.Append("{factoryId=").Append(_factoryId);
            sb.Append(", classId=").Append(_classId);
            sb.Append(", version=").Append(_version);
            sb.Append(", fieldDefinitions=").Append(_fieldDefinitionsMap.Values);
            sb.Append('}');
            return sb.ToString();
        }

        protected bool Equals(ClassDefinition other)
        {
            return _factoryId == other._factoryId && _classId == other._classId && _version == other._version &&
                   _fieldDefinitionsMap.Count == other._fieldDefinitionsMap.Count &&
                   !_fieldDefinitionsMap.Except(other._fieldDefinitionsMap).Any();
        }

        internal virtual void AddFieldDef(FieldDefinition fd)
        {
            _fieldDefinitionsMap[fd.Name] = fd;
        }

        internal virtual ICollection<IFieldDefinition> GetFieldDefinitions()
        {
            return _fieldDefinitionsMap.Values;
        }

        internal virtual void SetVersionIfNotSet(int version)
        {
            if (Version < 0)
            {
                _version = version;
            }
        }
    }
}
