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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ClassDefinition : IClassDefinition
    {
        private int factoryId;

        private int classId;

        private int version = -1;

        private readonly IDictionary<string, IFieldDefinition> fieldDefinitionsMap = new Dictionary<string, IFieldDefinition>();

        public ClassDefinition()
        {
        }

        public ClassDefinition(int factoryId, int classId, int version)
        {
            this.factoryId = factoryId;
            this.classId = classId;
            this.version = version;
        }

        internal virtual void AddFieldDef(FieldDefinition fd)
        {
            fieldDefinitionsMap[fd.GetName()] = fd;
        }

        public virtual IFieldDefinition GetField(string name)
        {
            IFieldDefinition val;
            return fieldDefinitionsMap.TryGetValue(name, out val) ? val : null;
        }

        public virtual IFieldDefinition GetField(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fieldDefinitionsMap.Count)
            {
                throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + fieldDefinitionsMap.Count);
            }
            foreach (IFieldDefinition fieldDefinition in fieldDefinitionsMap.Values)
            {
                if (fieldIndex == fieldDefinition.GetIndex())
                {
                    return fieldDefinition;
                }
            }
            throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + fieldDefinitionsMap.Count);
        }

        public virtual bool HasField(string fieldName)
        {
            return fieldDefinitionsMap.ContainsKey(fieldName);
        }

        public virtual ICollection<string> GetFieldNames()
        {
            return new HashSet<string>(fieldDefinitionsMap.Keys);
        }

        public virtual FieldType GetFieldType(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetFieldType();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetClassId();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        internal virtual ICollection<IFieldDefinition> GetFieldDefinitions()
        {
            return fieldDefinitionsMap.Values;
        }

        public virtual int GetFieldCount()
        {
            return fieldDefinitionsMap.Count;
        }

        public int GetFactoryId()
        {
            return factoryId;
        }

        public int GetClassId()
        {
            return classId;
        }

        public int GetVersion()
        {
            return version;
        }

        internal virtual void SetVersionIfNotSet(int version)
        {
            if (GetVersion() < 0)
            {
                this.version = version;
            }
        }

        protected bool Equals(ClassDefinition other)
        {
            return factoryId == other.factoryId && classId == other.classId && version == other.version && 
                fieldDefinitionsMap.Count == other.fieldDefinitionsMap.Count && 
                !fieldDefinitionsMap.Except(other.fieldDefinitionsMap).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClassDefinition) obj);
        }

        public override int GetHashCode()
        {
            int result = classId;
            result = 31 * result + version;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ClassDefinition");
            sb.Append("{factoryId=").Append(factoryId);
            sb.Append(", classId=").Append(classId);
            sb.Append(", version=").Append(version);
            sb.Append(", fieldDefinitions=").Append(fieldDefinitionsMap.Values);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
