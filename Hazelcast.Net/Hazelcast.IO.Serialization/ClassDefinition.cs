// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    internal class ClassDefinition : IClassDefinition
    {
        private readonly int _classId;
        private readonly int _factoryId;

        private readonly IDictionary<string, IFieldDefinition> _fieldDefinitionsMap =
            new Dictionary<string, IFieldDefinition>();

        private int _version = -1;

        public ClassDefinition()
        {
        }

        public ClassDefinition(int factoryId, int classId, int version)
        {
            _factoryId = factoryId;
            _classId = classId;
            _version = version;
        }

        public virtual IFieldDefinition GetField(string name)
        {
            IFieldDefinition val;
            return _fieldDefinitionsMap.TryGetValue(name, out val) ? val : null;
        }

        public virtual IFieldDefinition GetField(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= _fieldDefinitionsMap.Count)
            {
                throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + _fieldDefinitionsMap.Count);
            }
            foreach (var fieldDefinition in _fieldDefinitionsMap.Values)
            {
                if (fieldIndex == fieldDefinition.GetIndex())
                {
                    return fieldDefinition;
                }
            }
            throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + _fieldDefinitionsMap.Count);
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
                return fd.GetFieldType();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            var fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetClassId();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldCount()
        {
            return _fieldDefinitionsMap.Count;
        }

        public int GetFactoryId()
        {
            return _factoryId;
        }

        public int GetClassId()
        {
            return _classId;
        }

        public int GetVersion()
        {
            return _version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
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
            _fieldDefinitionsMap[fd.GetName()] = fd;
        }

        internal virtual ICollection<IFieldDefinition> GetFieldDefinitions()
        {
            return _fieldDefinitionsMap.Values;
        }

        internal virtual void SetVersionIfNotSet(int version)
        {
            if (GetVersion() < 0)
            {
                _version = version;
            }
        }
    }
}