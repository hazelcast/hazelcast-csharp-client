// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class FieldDefinition : IFieldDefinition
    {
        private readonly int _classId;
        private readonly int _factoryId;
        private readonly string _fieldName;
        private readonly int _index;
        private readonly FieldType _type;
        private readonly int _version;

        internal FieldDefinition()
        {
        }

        internal FieldDefinition(int index, string fieldName, FieldType type, int version)
            : this(index, fieldName, type, 0, 0, version)
        {
        }

        internal FieldDefinition(int index, string fieldName, FieldType type, int factoryId, int classId, int version)
        {
            _classId = classId;
            _type = type;
            _fieldName = fieldName;
            _index = index;
            _factoryId = factoryId;
            _version = version;
        }

        public virtual FieldType GetFieldType()
        {
            return _type;
        }

        public virtual string GetName()
        {
            return _fieldName;
        }

        public virtual int GetIndex()
        {
            return _index;
        }

        public virtual int GetFactoryId()
        {
            return _factoryId;
        }

        public virtual int GetClassId()
        {
            return _classId;
        }

        public virtual int GetVersion()
        {
            return _version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FieldDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _classId;
                hashCode = (hashCode*397) ^ _factoryId;
                hashCode = (hashCode*397) ^ (_fieldName != null ? _fieldName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _index;
                hashCode = (hashCode*397) ^ (int) _type;
                hashCode = (hashCode*397) ^ _version;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("FieldDefinition{");
            sb.Append("index=").Append(_index);
            sb.Append(", fieldName='").Append(_fieldName).Append('\'');
            sb.Append(", type=").Append(_type);
            sb.Append(", classId=").Append(_classId);
            sb.Append(", factoryId=").Append(_factoryId);
            sb.Append(", version=").Append(_version);
            sb.Append('}');
            return sb.ToString();
        }

        protected bool Equals(FieldDefinition other)
        {
            return _classId == other._classId && _factoryId == other._factoryId &&
                   string.Equals(_fieldName, other._fieldName) &&
                   _index == other._index && _type == other._type &&
                   _version == other._version;
        }

        internal virtual bool IsPortable()
        {
            return _type == FieldType.Portable || _type == FieldType.PortableArray;
        }
    }
}