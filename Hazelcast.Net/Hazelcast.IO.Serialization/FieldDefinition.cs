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

        //CHECKSTYLE:OFF
        //Generated equals method has too high NPath Complexity
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (FieldDefinition) o;
            if (classId != that.classId)
            {
                return false;
            }
            if (factoryId != that.factoryId)
            {
                return false;
            }
            if (fieldName != null ? !fieldName.Equals(that.fieldName) : that.fieldName != null)
            {
                return false;
            }
            if (type != that.type)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var result = fieldName != null ? fieldName.GetHashCode() : 0;
            result = 31*result + (type != null ? type.GetHashCode() : 0);
            result = 31*result + classId;
            result = 31*result + factoryId;
            return result;
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