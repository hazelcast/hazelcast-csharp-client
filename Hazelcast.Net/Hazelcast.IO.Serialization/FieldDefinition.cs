/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class FieldDefinition : IFieldDefinition
    {
        internal int classId;
        internal int factoryId;
        internal string fieldName;
        internal int index;
        internal FieldType type;

        internal FieldDefinition()
        {
        }

        internal FieldDefinition(int index, string fieldName, FieldType type)
            : this(index, fieldName, type, 0, 0)
        {
        }

        internal FieldDefinition(int index, string fieldName, FieldType type, int factoryId, int classId)
        {
            this.classId = classId;
            this.type = type;
            this.fieldName = fieldName;
            this.index = index;
            this.factoryId = factoryId;
        }

        public virtual FieldType GetFieldType()
        {
            return type;
        }

        public virtual string GetName()
        {
            return fieldName;
        }

        public virtual int GetIndex()
        {
            return index;
        }

        public virtual int GetFactoryId()
        {
            return factoryId;
        }

        public virtual int GetClassId()
        {
            return classId;
        }

        internal virtual bool IsPortable()
        {
            return type == FieldType.Portable || type == FieldType.PortableArray;
        }

        protected bool Equals(FieldDefinition other)
        {
            return classId == other.classId && factoryId == other.factoryId && string.Equals(fieldName, other.fieldName) && index == other.index && type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FieldDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = classId;
                hashCode = (hashCode*397) ^ factoryId;
                hashCode = (hashCode*397) ^ (fieldName != null ? fieldName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ index;
                hashCode = (hashCode*397) ^ (int) type;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("FieldDefinition{");
            sb.Append("index=").Append(index);
            sb.Append(", fieldName='").Append(fieldName).Append('\'');
            sb.Append(", type=").Append(type);
            sb.Append(", classId=").Append(classId);
            sb.Append(", factoryId=").Append(factoryId);
            sb.Append('}');
            return sb.ToString();
        }
    }
}